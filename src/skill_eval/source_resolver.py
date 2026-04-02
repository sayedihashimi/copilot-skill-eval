"""Resolve skill/plugin references to local paths, fetching remote sources as needed."""

from __future__ import annotations

import os
import re
import subprocess
from pathlib import Path

import click

from skill_eval.config import Configuration, EvalConfig, SkillReference
from skill_eval.source_config import SkillSource, SkillSourcesConfig, SourceType

_DEFAULT_CACHE_DIR = Path.home() / ".skill-eval" / "cache"


def _normalize_git_url(url: str) -> str:
    """Normalize a GitHub web URL to a clone-able git URL.

    Accepts:
      - https://github.com/user/repo          → https://github.com/user/repo.git
      - https://github.com/user/repo.git      → unchanged
      - git@github.com:user/repo.git           → unchanged
      - https://github.com/user/repo/tree/main → https://github.com/user/repo.git
    """
    # Strip trailing slash
    url = url.rstrip("/")

    # SSH URLs are fine as-is
    if url.startswith("git@"):
        return url

    # Strip /tree/<branch> or /blob/<branch> suffixes from GitHub web URLs
    url = re.sub(r"/(?:tree|blob)/[^/]+(?:/.*)?$", "", url)

    # Ensure .git suffix for HTTPS URLs
    if url.startswith("https://") and not url.endswith(".git"):
        url += ".git"

    return url


def _safe_dir_name(name: str) -> str:
    """Sanitize a source name for use as a directory name."""
    return re.sub(r"[^\w\-.]", "_", name)


class SourceResolver:
    """Resolves skill/plugin references to local filesystem paths.

    For legacy plain-string paths, resolves relative to project_root.
    For named source references, fetches or updates git repos and resolves
    within the cached clone.
    """

    def __init__(
        self,
        sources_config: SkillSourcesConfig | None,
        project_root: Path,
        cache_dir: Path | None = None,
    ) -> None:
        self._sources = sources_config
        self._project_root = project_root.resolve()
        self._cache_dir = (cache_dir or _DEFAULT_CACHE_DIR).resolve()
        self._resolved_cache: dict[str, Path] = {}

    @property
    def cache_dir(self) -> Path:
        return self._cache_dir

    def resolve(self, ref: SkillReference) -> Path:
        """Resolve a single skill/plugin reference to an absolute local path."""
        if ref.local_path:
            return self._project_root / ref.local_path

        if not ref.source:
            raise ValueError("SkillReference has neither local_path nor source set")

        if not self._sources:
            raise ValueError(
                f"Skill reference uses source '{ref.source}' but no "
                "skill-sources.yaml was loaded. Create a skill-sources.yaml "
                "file or use local paths."
            )

        source = self._sources.get_source(ref.source)
        source_root = self._ensure_source(source)

        # Apply source-level path first, then ref-level path
        result = source_root / source.path
        if ref.path:
            result = result / ref.path

        return result.resolve()

    def resolve_all(self, config: EvalConfig) -> dict[str, ResolvedConfiguration]:
        """Resolve all skill/plugin references for every configuration."""
        resolved: dict[str, ResolvedConfiguration] = {}
        for cfg in config.configurations:
            resolved[cfg.name] = ResolvedConfiguration(
                name=cfg.name,
                label=cfg.label,
                skill_paths=[self.resolve(ref) for ref in cfg.skills],
                plugin_paths=[self.resolve(ref) for ref in cfg.plugins],
            )
        return resolved

    def _ensure_source(self, source: SkillSource) -> Path:
        """Ensure a source is available locally, cloning/updating as needed."""
        if source.name in self._resolved_cache:
            return self._resolved_cache[source.name]

        if source.type == SourceType.LOCAL:
            path = self._project_root / (source.path if source.path != "." else "")
            self._resolved_cache[source.name] = path.resolve()
            return self._resolved_cache[source.name]

        # Git source
        cache_path = self._cache_dir / _safe_dir_name(source.name)

        if cache_path.exists() and (cache_path / ".git").exists():
            self._update_git_source(source, cache_path)
        else:
            self._clone_git_source(source, cache_path)

        self._resolved_cache[source.name] = cache_path
        return cache_path

    def _clone_git_source(self, source: SkillSource, dest: Path) -> None:
        """Clone a git source into the cache."""
        assert source.url is not None
        url = _normalize_git_url(source.url)

        dest.parent.mkdir(parents=True, exist_ok=True)

        cmd = ["git", "clone"]
        if source.ref:
            cmd.extend(["--branch", source.ref])
        cmd.extend(["--depth", "1", url, str(dest)])

        click.echo(f"  📥 Cloning {source.name} from {url}...")
        result = subprocess.run(cmd, capture_output=True, text=True)
        if result.returncode != 0:
            raise RuntimeError(
                f"Failed to clone source '{source.name}' from {url}:\n"
                f"{result.stderr.strip()}"
            )
        click.echo(f"  ✅ Cloned {source.name}")

    def _update_git_source(self, source: SkillSource, cache_path: Path) -> None:
        """Update an existing git clone."""
        click.echo(f"  🔄 Updating {source.name}...")

        if source.ref:
            # Fetch and checkout specific ref
            subprocess.run(
                ["git", "fetch", "origin"],
                cwd=cache_path,
                capture_output=True,
                text=True,
            )
            subprocess.run(
                ["git", "checkout", source.ref],
                cwd=cache_path,
                capture_output=True,
                text=True,
            )
            result = subprocess.run(
                ["git", "pull", "--ff-only"],
                cwd=cache_path,
                capture_output=True,
                text=True,
            )
        else:
            result = subprocess.run(
                ["git", "pull", "--ff-only"],
                cwd=cache_path,
                capture_output=True,
                text=True,
            )

        if result.returncode != 0:
            click.echo(f"  ⚠️  Pull failed for {source.name}, using cached version")
        else:
            click.echo(f"  ✅ Updated {source.name}")


class ResolvedConfiguration:
    """A configuration with all skill/plugin references resolved to local paths."""

    def __init__(
        self,
        name: str,
        label: str,
        skill_paths: list[Path],
        plugin_paths: list[Path],
    ) -> None:
        self.name = name
        self.label = label
        self.skill_paths = skill_paths
        self.plugin_paths = plugin_paths

    @property
    def has_skills_or_plugins(self) -> bool:
        return bool(self.skill_paths or self.plugin_paths)

    @property
    def skill_names(self) -> list[str]:
        return [p.name for p in self.skill_paths]

    @property
    def plugin_names(self) -> list[str]:
        return [p.name for p in self.plugin_paths]
