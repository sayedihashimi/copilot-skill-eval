"""Schema and loader for skill-sources.yaml."""

from __future__ import annotations

from enum import Enum
from pathlib import Path

import yaml
from pydantic import BaseModel, field_validator, model_validator


class SourceType(str, Enum):
    """Type of skill/plugin source."""

    GIT = "git"
    LOCAL = "local"


class SkillSource(BaseModel):
    """A single skill/plugin source definition."""

    name: str
    type: SourceType
    url: str | None = None
    ref: str | None = None  # branch, tag, or commit (git only)
    path: str = "."  # subfolder within the source repo/directory

    @model_validator(mode="after")
    def validate_source(self) -> SkillSource:
        if self.type == SourceType.GIT and not self.url:
            raise ValueError(
                f"Source '{self.name}': 'url' is required for git sources"
            )
        if self.type == SourceType.LOCAL and self.url:
            raise ValueError(
                f"Source '{self.name}': 'url' should not be set for local sources"
            )
        if self.type == SourceType.LOCAL and self.ref:
            raise ValueError(
                f"Source '{self.name}': 'ref' should not be set for local sources"
            )
        return self


class SkillSourcesConfig(BaseModel):
    """Root configuration model for skill-sources.yaml."""

    cache_dir: str | None = None  # override default cache directory
    sources: list[SkillSource]

    @field_validator("sources")
    @classmethod
    def unique_source_names(cls, v: list[SkillSource]) -> list[SkillSource]:
        names = [s.name for s in v]
        dupes = [n for n in names if names.count(n) > 1]
        if dupes:
            raise ValueError(f"Duplicate source names: {', '.join(set(dupes))}")
        return v

    def get_source(self, name: str) -> SkillSource:
        """Look up a source by name."""
        for s in self.sources:
            if s.name == name:
                return s
        available = ", ".join(s.name for s in self.sources)
        raise KeyError(
            f"Source '{name}' not found in skill-sources.yaml. "
            f"Available sources: {available}"
        )


def load_skill_sources(path: Path) -> SkillSourcesConfig:
    """Load and validate skill-sources.yaml from the given path."""
    if not path.exists():
        raise FileNotFoundError(f"Skill sources file not found: {path}")

    with open(path) as f:
        raw = yaml.safe_load(f)

    if raw is None:
        raise ValueError(f"Skill sources file is empty: {path}")

    return SkillSourcesConfig.model_validate(raw)
