"""Render Jinja2 prompt templates from eval.yaml configuration."""

from __future__ import annotations

from pathlib import Path

from jinja2 import Environment, FileSystemLoader

from skill_eval.config import Configuration, EvalConfig, Scenario

# Templates are bundled inside the package for standalone installs.
# Also check the repo-root templates/ for development convenience.
_PACKAGE_TEMPLATES = Path(__file__).resolve().parent / "templates"
_REPO_TEMPLATES = Path(__file__).resolve().parent.parent.parent / "templates"


def _get_env(project_root: Path) -> Environment:
    """Build a Jinja2 environment that searches project-local then bundled templates."""
    search_paths = []

    local_templates = project_root / "templates"
    if local_templates.is_dir():
        search_paths.append(str(local_templates))

    # Prefer in-package templates (works with pip/pipx installs)
    if _PACKAGE_TEMPLATES.is_dir():
        search_paths.append(str(_PACKAGE_TEMPLATES))
    # Fall back to repo-root templates (development mode)
    elif _REPO_TEMPLATES.is_dir():
        search_paths.append(str(_REPO_TEMPLATES))

    if not search_paths:
        raise FileNotFoundError(
            "No templates directory found. Expected templates/ in your project root "
            f"or the bundled templates at {_PACKAGE_TEMPLATES}"
        )

    return Environment(
        loader=FileSystemLoader(search_paths),
        keep_trailing_newline=True,
        trim_blocks=True,
        lstrip_blocks=True,
    )


def render_generate_prompt(
    config: EvalConfig,
    configuration_name: str,
    project_root: Path,
    scenario: Scenario | None = None,
    run_id: int | None = None,
) -> str:
    """Render the generation prompt for a specific configuration.

    If *scenario* is provided, renders a single-app prompt using
    ``create-single-app.md.j2``.  Otherwise falls back to the
    multi-app ``create-all-apps.md.j2`` template.

    If *run_id* is provided, output goes under a ``run-{N}`` subdirectory.
    """
    cfg = next(c for c in config.configurations if c.name == configuration_name)
    has_skills = bool(cfg.skills or cfg.plugins)
    env = _get_env(project_root)

    output_dir = f"{config.output.directory}/{configuration_name}"
    if run_id is not None:
        output_dir = f"{output_dir}/run-{run_id}"

    if scenario is not None:
        # Use analysis template when the scenario includes existing directories
        if scenario.include_directories:
            template = env.get_template("analyze-code.md.j2")
            return template.render(
                scenario=scenario,
                output_directory=output_dir,
                has_skills=has_skills,
                include_directories=scenario.include_directories,
            )
        template = env.get_template("create-single-app.md.j2")
        return template.render(
            scenario=scenario,
            output_directory=output_dir,
            has_skills=has_skills,
        )

    template = env.get_template("create-all-apps.md.j2")
    return template.render(
        scenarios=config.scenarios,
        output_directory=output_dir,
        has_skills=has_skills,
    )


def render_analyze_prompt(
    config: EvalConfig,
    project_root: Path,
    run_id: int | None = None,
) -> str:
    """Render the analysis prompt from configured dimensions.

    If *run_id* is provided, the prompt targets output under ``run-{N}``
    subdirectories for each configuration.

    Selects ``analyze-text.md.j2`` for text_output evals, otherwise
    ``analyze.md.j2``.
    """
    env = _get_env(project_root)
    template_name = "analyze-text.md.j2" if config.is_text_output else "analyze.md.j2"
    template = env.get_template(template_name)

    output_directory = config.output.directory
    run_suffix = f"/run-{run_id}" if run_id is not None else ""

    return template.render(
        scenarios=config.scenarios,
        configurations=config.configurations,
        dimensions=config.dimensions,
        output_directory=output_directory,
        reports_directory=config.output.reports_directory,
        analysis_file=config.output.analysis_file,
        run_suffix=run_suffix,
    )


def render_scenario_template(scenario_name: str, project_root: Path) -> str:
    """Render a starter scenario prompt template."""
    env = _get_env(project_root)
    template = env.get_template("scenario.prompt.md.j2")
    return template.render(scenario_name=scenario_name)


def render_improvement_prompt(
    config: EvalConfig,
    configuration: Configuration,
    project_root: Path,
    skill_paths: list[Path],
    plugin_paths: list[Path],
) -> str:
    """Render the improvement suggestions prompt for a specific configuration.

    *skill_paths* and *plugin_paths* are resolved absolute paths to the
    skill/plugin directories for this configuration.
    """
    env = _get_env(project_root)
    template = env.get_template("suggest-improvements.md.j2")

    reports_dir = config.output.reports_directory
    improvements_file = config.output.improvements_file_pattern.format(
        config=configuration.name
    )

    # Build paths to available data files
    build_notes_path = f"{reports_dir}/{config.output.notes_file}"
    scores_data_path = f"{reports_dir}/{config.output.scores_data_file}"
    verification_data_path = f"{reports_dir}/{config.output.verification_data_file}"

    # Check which data files actually exist
    build_notes_exists = (project_root / build_notes_path).exists()
    verification_data_exists = (project_root / verification_data_path).exists()
    scores_data_exists = (project_root / scores_data_path).exists()

    other_configurations = [
        c for c in config.configurations if c.name != configuration.name
    ]

    return template.render(
        config_name=configuration.name,
        config_label=configuration.label,
        skill_paths=[str(p) for p in skill_paths],
        plugin_paths=[str(p) for p in plugin_paths],
        dimensions=config.dimensions,
        other_configurations=other_configurations,
        reports_directory=reports_dir,
        analysis_file=config.output.analysis_file,
        improvements_file=improvements_file,
        build_notes_path=build_notes_path if build_notes_exists else None,
        verification_data_path=verification_data_path if verification_data_exists else None,
        scores_data_path=scores_data_path if scores_data_exists else None,
    )
