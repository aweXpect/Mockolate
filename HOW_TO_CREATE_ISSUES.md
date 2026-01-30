# How to Create Documentation Issues for Mockolate

This guide explains how to create GitHub issues for the identified documentation gaps in Mockolate.

## Quick Start

You have **3 options** to create the issues:

### Option 1: Automated Script (Recommended) ⚡

If you have the GitHub CLI (`gh`) installed and authenticated:

```bash
./create_documentation_issues.sh
```

This will automatically create all 12 issues with proper titles, labels, and descriptions.

**Prerequisites:**
- Install GitHub CLI: https://cli.github.com/
- Authenticate: `gh auth login`
- You must have write access to the `aweXpect/Mockolate` repository

### Option 2: Use the JSON File 📋

Use the `documentation_issues.json` file with the GitHub API or your favorite tool:

**Using GitHub API:**
```bash
# For each issue in documentation_issues.json
curl -X POST \
  -H "Accept: application/vnd.github+json" \
  -H "Authorization: Bearer YOUR_GITHUB_TOKEN" \
  https://api.github.com/repos/aweXpect/Mockolate/issues \
  -d '{
    "title": "Issue title from JSON",
    "body": "Issue body from JSON",
    "labels": ["documentation", "enhancement"]
  }'
```

**Using a script:**
You can write a simple script to parse `documentation_issues.json` and create issues via the API.

### Option 3: Manual Creation 📝

Create issues manually from the detailed information in `DOCUMENTATION_GAPS.md`:

1. Go to https://github.com/aweXpect/Mockolate/issues
2. Click "New Issue"
3. For each of the 12 feature groups:
   - Copy the title from SUMMARY.md table or DOCUMENTATION_GAPS.md
   - Copy the suggested documentation content as the issue body
   - Add labels: `documentation`, `enhancement`
   - Optionally add a priority label: `priority: high`, `priority: medium`, `priority: low`
   - Click "Submit new issue"

## Issue Breakdown

### High Priority Issues (Create These First) 🔴

1. **Document Mock.Wrap feature**
   - Status: Complete feature not documented
   - User Impact: High - common pattern for wrapping real objects

2. **Document delegate mocking feature**
   - Status: Mentioned in example but not documented
   - User Impact: High - delegates are very common in C#

3. **Document advanced parameter matching (ref/out/span parameters)**
   - Status: Partial - missing modern C# features
   - User Impact: High - critical for ref/out parameters

4. **Document protected member support**
   - Status: Complete feature not documented
   - User Impact: High - needed for testing classes

### Medium Priority Issues 🟡

5. **Document advanced callback features**
   - Enhances existing callback documentation
   - Adds .When(), .For(), .Only(), .InParallel()

6. **Document advanced property and indexer features**
   - Completes existing property/indexer sections
   - Adds advanced return and callback options

7. **Document additional verification count methods**
   - Adds .Between() and .Times() predicates
   - Rounds out verification options

8. **Document extended HttpClient features**
   - Completes HttpClient section
   - Adds PUT/DELETE/PATCH, advanced matchers

### Low Priority Issues 🟢

9. **Document interaction tracking features**
   - Advanced debugging feature
   - .Interactions, .ClearAllInteractions(), MockMonitor

10. **Document advanced mock behavior options**
    - Edge case configuration options
    - Advanced initialization patterns

11. **Document special method overrides (ToString, Equals, GetHashCode)**
    - Rare use case
    - Useful for specific scenarios

12. **Document async exception handling with ThrowsAsync**
    - Minor completion of async documentation
    - .ThrowsAsync() variants

## Verification

After creating the issues, verify:

1. All 12 issues are created
2. Each has labels: `documentation`, `enhancement`
3. Each has a clear description with examples
4. Issues are linked to this PR (if applicable)

Check issues at: https://github.com/aweXpect/Mockolate/issues

## Files Reference

- **SUMMARY.md** - Quick reference table and overview
- **DOCUMENTATION_GAPS.md** - Detailed analysis with all examples (16KB)
- **documentation_issues.json** - JSON format for automation (14KB)
- **create_documentation_issues.sh** - Bash script for GitHub CLI (16KB)
- **HOW_TO_CREATE_ISSUES.md** - This file

## Tips

- **Start with high-priority issues** - They have the most user impact
- **Create in batches** - High priority first, then medium, then low
- **Reference DOCUMENTATION_GAPS.md** - It has all the detailed content
- **Link related issues** - Consider creating an epic/milestone for all docs issues
- **Add to project board** - If you use GitHub Projects, add these to your docs backlog

## Questions?

- Review DOCUMENTATION_GAPS.md for complete details
- Check SUMMARY.md for the quick overview
- See documentation_issues.json for the exact issue content

## Impact

These 12 issues address approximately **40% of Mockolate features** that are currently undocumented. 

Addressing the **4 high-priority issues** alone would document the most commonly needed features and significantly improve the user experience.

---

**Last Updated**: 2026-01-30
**Author**: Documentation Gap Analysis
**Repository**: aweXpect/Mockolate
