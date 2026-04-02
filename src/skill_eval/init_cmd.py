"""Interactive initialization for a new skill evaluation project."""

from __future__ import annotations

from pathlib import Path

import click
import yaml

from skill_eval.prompt_renderer import render_scenario_template


def run_init(project_root: Path) -> None:
    """Interactively create eval.yaml, skill-sources.yaml, and starter scenario prompts."""
    eval_path = project_root / "eval.yaml"
    sources_path = project_root / "skill-sources.yaml"

    if eval_path.exists():
        if not click.confirm("eval.yaml already exists. Overwrite?", default=False):
            click.echo("Aborted.")
            return

    click.echo("\n🚀 Copilot Skill Evaluation — Project Setup\n")

    # Project metadata
    name = click.prompt("Evaluation name", default="My Skill Evaluation")
    description = click.prompt(
        "Short description",
        default="Evaluate how my custom skills improve code generation",
    )

    # Tech stack (informational — affects scenario templates)
    tech_stack = click.prompt(
        "Tech stack (e.g., ASP.NET Core, React, Go, Python Flask)",
        default="ASP.NET Core",
    )

    # Scenarios
    click.echo("\n📋 Scenarios — realistic apps to generate for comparison")
    scenarios = []
    while True:
        scenario_name = click.prompt(
            f"  Scenario {len(scenarios) + 1} name (or 'done' to finish)",
            default="done" if len(scenarios) >= 2 else "",
        )
        if scenario_name.lower() == "done" and len(scenarios) >= 1:
            break
        if scenario_name.lower() == "done":
            click.echo("  At least one scenario is required.")
            continue

        scenario_desc = click.prompt(f"  Brief description of {scenario_name}", default="")
        scenarios.append({"name": scenario_name, "description": scenario_desc})

    # Configurations and skill sources
    click.echo("\n⚙️  Configurations — skill sets to compare")
    click.echo("  A baseline configuration will be created automatically.")
    configurations = [
        {"name": "no-skills", "label": "Baseline (default Copilot)", "skills": [], "plugins": []},
    ]
    skill_sources: list[dict] = []
    source_names: set[str] = set()

    while True:
        config_name = click.prompt(
            f"  Configuration {len(configurations) + 1} name (or 'done' to finish)",
            default="done" if len(configurations) >= 2 else "",
        )
        if config_name.lower() == "done" and len(configurations) >= 2:
            break
        if config_name.lower() == "done":
            click.echo("  At least two configurations (baseline + one skill) are required.")
            continue

        config_label = click.prompt(f"  Display label for '{config_name}'", default=config_name)

        skills, new_sources = _prompt_for_refs(
            "skill", config_name, skill_sources, source_names,
        )
        skill_sources.extend(new_sources)

        plugins, new_sources = _prompt_for_refs(
            "plugin", config_name, skill_sources, source_names,
        )
        skill_sources.extend(new_sources)

        configurations.append({
            "name": config_name,
            "label": config_label,
            "skills": skills,
            "plugins": plugins,
        })

    # Verification commands
    click.echo(f"\n🔨 Verification — how to build/run {tech_stack} projects")
    build_cmd = click.prompt("  Build command", default=_default_build_cmd(tech_stack))
    run_cmd = click.prompt(
        "  Run command (or empty to skip run verification)",
        default=_default_run_cmd(tech_stack),
    )

    # Dimensions
    click.echo("\n📊 Analysis dimensions — quality criteria to evaluate")
    click.echo("  You can add more later by editing eval.yaml.")
    dimensions = _suggest_dimensions(tech_stack)
    click.echo(f"  Suggested {len(dimensions)} dimensions for {tech_stack}.")

    # Build the config dict
    verification: dict = {
        "build": {"command": build_cmd},
    }
    if run_cmd:
        verification["run"] = {"command": run_cmd, "timeout_seconds": 15}

    config = {
        "name": name,
        "description": description,
        "scenarios": [
            {
                "name": s["name"],
                "prompt": f"prompts/scenarios/{_slugify(s['name'])}.prompt.md",
                "description": s["description"],
            }
            for s in scenarios
        ],
        "configurations": configurations,
        "verification": verification,
        "dimensions": dimensions,
        "output": {
            "directory": "output",
            "reports_directory": "reports",
            "analysis_file": "analysis.md",
            "notes_file": "build-notes.md",
        },
    }

    # Write eval.yaml
    eval_path.write_text(
        yaml.dump(config, default_flow_style=False, sort_keys=False, allow_unicode=True),
        encoding="utf-8",
    )
    click.echo(f"\n✅ Created: {eval_path}")

    # Write skill-sources.yaml if any sources were defined
    if skill_sources:
        sources_data = {"sources": skill_sources}
        sources_path.write_text(
            yaml.dump(sources_data, default_flow_style=False, sort_keys=False, allow_unicode=True),
            encoding="utf-8",
        )
        click.echo(f"✅ Created: {sources_path}")

    # Create scenario prompt files
    prompts_dir = project_root / "prompts" / "scenarios"
    prompts_dir.mkdir(parents=True, exist_ok=True)

    for s in scenarios:
        prompt_file = prompts_dir / f"{_slugify(s['name'])}.prompt.md"
        if not prompt_file.exists():
            content = render_scenario_template(s["name"], project_root)
            prompt_file.write_text(content, encoding="utf-8")
            click.echo(f"✅ Created: {prompt_file}")
        else:
            click.echo(f"⏭️  Exists:  {prompt_file}")

    # Create skills directory
    skills_dir = project_root / "skills"
    skills_dir.mkdir(exist_ok=True)

    # Optionally generate CI workflow
    setup_ci = click.confirm(
        "\n🔄 Generate a GitHub Actions workflow for CI?", default=False,
    )
    if setup_ci:
        _generate_ci_workflow(project_root, config, has_sources=bool(skill_sources))

    # Print next steps
    source_step = ""
    if skill_sources:
        source_step = """
  2. Review skill-sources.yaml — verify git URLs and paths.
     Sources will be cloned automatically on first run.
"""
    else:
        source_step = """
  2. Copy your skill(s) into the skills/ directory,
     or create a skill-sources.yaml with remote git references.
"""

    click.echo(f"""
{'=' * 60}
🎉 Setup complete!

Next steps:
  1. Edit the scenario prompts in prompts/scenarios/
     Describe each app's entities, business rules, and endpoints.
{source_step}
  3. Review eval.yaml and adjust dimensions as needed.

  4. Run the evaluation:
     skill-eval run

  Or use the agent:
     @skill-eval run the evaluation
{'=' * 60}
""")


def _prompt_for_refs(
    ref_type: str,
    config_name: str,
    existing_sources: list[dict],
    source_names: set[str],
) -> tuple[list, list[dict]]:
    """Prompt user for skill or plugin references, returning refs and any new sources.

    Returns (refs_for_config, new_sources_to_add).
    """
    refs: list = []
    new_sources: list[dict] = []

    while True:
        if refs:
            prompt_text = f"  Another {ref_type} for '{config_name}' (or empty to finish)"
        else:
            prompt_text = f"  {ref_type.capitalize()} for '{config_name}' (or empty for none)"
        entry = click.prompt(prompt_text, default="")
        if not entry:
            break

        location = click.prompt(
            f"    Is '{entry}' local or remote?",
            type=click.Choice(["local", "remote", "existing-source"]),
            default="remote",
        )

        if location == "local":
            refs.append(entry)  # plain string = local path
        elif location == "existing-source":
            # Reference a previously defined source
            if not existing_sources and not new_sources:
                click.echo("    No sources defined yet. Defining a new one.")
                location = "remote"
            else:
                all_names = [s["name"] for s in existing_sources + new_sources]
                click.echo(f"    Available sources: {', '.join(all_names)}")
                source_name = click.prompt("    Source name", type=click.Choice(all_names))
                sub_path = click.prompt("    Sub-path within source (or empty)", default="")
                ref_dict: dict = {"source": source_name}
                if sub_path:
                    ref_dict["path"] = sub_path
                refs.append(ref_dict)
                continue

        if location == "remote":
            # Create a new source
            source_name = click.prompt(
                f"    Source name (unique identifier)", default=entry,
            )
            while source_name in source_names:
                click.echo(f"    Name '{source_name}' already used.")
                source_name = click.prompt("    Choose another name")

            git_url = click.prompt(f"    Git URL for '{source_name}'")
            git_ref = click.prompt(
                "    Branch/tag (or empty for default)", default="",
            )
            sub_path = click.prompt(
                "    Subfolder within repo (or empty for root)", default="",
            )

            source: dict = {
                "name": source_name,
                "type": "git",
                "url": git_url,
            }
            if git_ref:
                source["ref"] = git_ref
            if sub_path:
                source["path"] = sub_path

            new_sources.append(source)
            source_names.add(source_name)

            # Build the ref for the configuration
            ref_dict = {"source": source_name}
            refs.append(ref_dict)

    return refs, new_sources


def _generate_ci_workflow(
    project_root: Path,
    config: dict,
    has_sources: bool,
) -> None:
    """Generate a GitHub Actions workflow file."""
    from jinja2 import Environment, FileSystemLoader

    from skill_eval.prompt_renderer import _PACKAGE_TEMPLATES, _REPO_TEMPLATES

    template_dir = _PACKAGE_TEMPLATES if _PACKAGE_TEMPLATES.is_dir() else _REPO_TEMPLATES
    env = Environment(
        loader=FileSystemLoader(str(template_dir)),
        keep_trailing_newline=True,
    )
    template = env.get_template("ci-workflow.yml.j2")

    rendered = template.render(
        config_path="eval.yaml",
        skill_sources_path="skill-sources.yaml" if has_sources else None,
        runs_on="ubuntu-latest",
        python_version="3.12",
        schedule=None,
        timeout_minutes=120,
        reports_directory=config.get("output", {}).get("reports_directory", "reports"),
        output_directory=config.get("output", {}).get("directory", "output"),
    )

    workflow_dir = project_root / ".github" / "workflows"
    workflow_dir.mkdir(parents=True, exist_ok=True)
    workflow_path = workflow_dir / "skill-eval.yml"
    workflow_path.write_text(rendered, encoding="utf-8")
    click.echo(f"✅ Created: {workflow_path}")


def _slugify(name: str) -> str:
    """Convert a name to a URL-friendly slug."""
    return name.lower().replace(" ", "-").replace("_", "-")


def _default_build_cmd(tech_stack: str) -> str:
    """Suggest a build command based on tech stack."""
    stack = tech_stack.lower()
    if "dotnet" in stack or "asp.net" in stack or ".net" in stack:
        return "dotnet build"
    if "react" in stack or "next" in stack or "node" in stack or "npm" in stack:
        return "npm run build"
    if "go" in stack or "golang" in stack:
        return "go build ./..."
    if "python" in stack or "flask" in stack or "django" in stack or "fastapi" in stack:
        return "python -m py_compile"
    return "make build"


def _default_run_cmd(tech_stack: str) -> str:
    """Suggest a run command based on tech stack."""
    stack = tech_stack.lower()
    if "dotnet" in stack or "asp.net" in stack or ".net" in stack:
        return "dotnet run"
    if "react" in stack or "next" in stack or "node" in stack or "npm" in stack:
        return "npm start"
    if "go" in stack or "golang" in stack:
        return "go run ."
    if "python" in stack or "flask" in stack or "django" in stack or "fastapi" in stack:
        return "python -m flask run"
    return ""


def _suggest_dimensions(tech_stack: str) -> list[dict]:
    """Suggest analysis dimensions based on tech stack."""
    # Universal dimensions that apply to any stack
    dimensions = [
        {
            "name": "Code Organization",
            "description": "How files and modules are structured",
            "what_to_look_for": "Check folder structure, file naming, separation of concerns, module boundaries.",
            "why_it_matters": "Good organization improves maintainability and discoverability.",
        },
        {
            "name": "Error Handling",
            "description": "How errors and exceptions are handled",
            "what_to_look_for": "Check for consistent error handling patterns, custom error types, proper HTTP status codes or error responses.",
            "why_it_matters": "Consistent error handling is critical for reliability and debugging.",
        },
        {
            "name": "Type Safety",
            "description": "Use of type annotations, strict types, and immutability",
            "what_to_look_for": "Check for type annotations, strict mode settings, use of immutable data structures.",
            "why_it_matters": "Type safety catches bugs at compile/lint time and improves code clarity.",
        },
        {
            "name": "Input Validation",
            "description": "How user input and request data is validated",
            "what_to_look_for": "Check for validation libraries, data annotations, schema validation, or manual checks.",
            "why_it_matters": "Proper validation prevents security issues and improves data integrity.",
        },
        {
            "name": "Testing Support",
            "description": "Whether the generated code is structured for testability",
            "what_to_look_for": "Check for dependency injection, interface abstractions, separation of business logic from I/O.",
            "why_it_matters": "Testable code is a strong signal of good architecture.",
        },
        # Security dimensions
        {
            "name": "Security Vulnerability Scan",
            "description": "Whether generated code is free from common security vulnerabilities",
            "what_to_look_for": "Scan for SQL injection, XSS, CSRF, insecure deserialization, hardcoded secrets, and OWASP Top 10 issues.",
            "why_it_matters": "Security vulnerabilities in generated code can lead to serious production incidents.",
            "tier": "critical",
            "evaluation_method": "automated",
        },
        {
            "name": "Input Validation Coverage",
            "description": "How thoroughly user input is validated across all entry points",
            "what_to_look_for": "Check that all API endpoints, form handlers, and data entry points validate and sanitize input.",
            "why_it_matters": "Incomplete input validation is a primary source of security and data integrity issues.",
            "tier": "critical",
            "evaluation_method": "hybrid",
        },
        # Functional Correctness dimensions
        {
            "name": "Endpoint Completeness",
            "description": "Whether all required endpoints (or pages for Razor Pages) are implemented",
            "what_to_look_for": "Verify all CRUD operations and business endpoints are present and correctly routed.",
            "why_it_matters": "Missing endpoints mean the application cannot fulfill its functional requirements.",
            "tier": "critical",
            "evaluation_method": "hybrid",
        },
        {
            "name": "Business Rule Implementation",
            "description": "Whether business logic and domain rules are correctly implemented",
            "what_to_look_for": "Check that validation rules, calculations, state transitions, and domain constraints match requirements.",
            "why_it_matters": "Incorrect business rules lead to wrong application behavior regardless of code quality.",
            "tier": "critical",
            "evaluation_method": "llm",
        },
        # Error Response Conformance
        {
            "name": "Error Response Conformance",
            "description": "Whether error responses follow a consistent, standard format",
            "what_to_look_for": "Check for consistent error response structure, proper HTTP status codes, and RFC 7807 problem details or equivalent.",
            "why_it_matters": "Consistent error responses improve API usability and client-side error handling.",
            "tier": "high",
            "evaluation_method": "hybrid",
        },
    ]

    stack = tech_stack.lower()

    # Stack-specific dimensions
    if "dotnet" in stack or "asp.net" in stack or ".net" in stack:
        dimensions.extend([
            {
                "name": "API Style",
                "description": "Controllers (MVC) vs Minimal APIs",
                "what_to_look_for": "Check Program.cs and endpoint files for MapGet/MapPost (minimal) vs [ApiController] classes.",
                "why_it_matters": "Minimal APIs have lower overhead and are the modern .NET default.",
                "tier": "high",
            },
            {
                "name": "CancellationToken Propagation",
                "description": "Whether tokens are forwarded through all layers",
                "what_to_look_for": "Check endpoint handlers for CancellationToken parameters. Trace through service methods to EF Core calls.",
                "why_it_matters": "Critical for production — prevents wasted server resources on cancelled requests.",
                "tier": "critical",
            },
            {
                "name": "Sealed Types",
                "description": "Whether classes and records are declared sealed",
                "what_to_look_for": "Check class/record declarations for the sealed modifier.",
                "why_it_matters": "Sealed types enable JIT optimizations and signal design intent.",
                "tier": "medium",
            },
        ])
    elif "react" in stack or "next" in stack:
        dimensions.extend([
            {
                "name": "Component Architecture",
                "description": "Component composition patterns and reusability",
                "what_to_look_for": "Check for atomic design, compound components, render props, custom hooks.",
                "why_it_matters": "Good component architecture enables reuse and simplifies testing.",
            },
            {
                "name": "State Management",
                "description": "How application state is managed",
                "what_to_look_for": "Check for Context, Redux, Zustand, React Query, or local state patterns.",
                "why_it_matters": "State management approach affects performance, complexity, and maintainability.",
            },
        ])
    elif "go" in stack or "golang" in stack:
        dimensions.extend([
            {
                "name": "Error Handling Idioms",
                "description": "Go-idiomatic error handling patterns",
                "what_to_look_for": "Check for error wrapping, sentinel errors, custom error types, proper error checking.",
                "why_it_matters": "Idiomatic Go error handling improves debuggability and API clarity.",
            },
            {
                "name": "Interface Design",
                "description": "Interface usage and consumer-side definition",
                "what_to_look_for": "Check if interfaces are defined at the consumer site, kept small, and used for testing.",
                "why_it_matters": "Small, consumer-defined interfaces are a Go best practice for loose coupling.",
            },
        ])

    return dimensions
