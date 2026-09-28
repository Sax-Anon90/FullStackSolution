# Put InventoryHub on GitHub

## Before uploading

- Build and run both applications.
- Run the checks in TESTING.md and review the source.
- Complete a real Copilot review and update REFLECTION.md.
- Capture your own test/performance evidence. A screenshot may help reviewers but is not explicitly required by the supplied rubric.

## Create and push

1. Sign into your GitHub account and create an empty repository named `InventoryHub`. Choose visibility according to your course's peer-review instructions. Do not initialize it with a README, license or gitignore because this project already has files.
2. Open a terminal in the extracted project folder (the folder containing the solution).
3. Replace `YOUR_USERNAME` below with your own GitHub username, then run:

```shell
git init
git add .
git commit -m "Build InventoryHub full-stack integration project"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/InventoryHub.git
git push -u origin main
```

Authenticate using GitHub's supported sign-in flow if prompted. If Git needs your identity, set your own user.name and user.email before committing. Do not paste credentials into source files.

4. On GitHub, check that ClientApp, ServerApp, Shared, the solution and documentation are visible at the repository root.
5. Open Actions and inspect “Build and check InventoryHub”. It restores/builds the solution, runs service checks, publishes the Blazor client and runs live API checks. Review failures before submitting; do not assume a push proves the build works.
6. Submit the repository URL through the course peer-review form. Ensure reviewers have access to it.

GitHub Pages cannot run this ASP.NET Core API. The supplied activity calls for a source repository; a separate cloud deployment is not required.

## Final checklist

- [ ] Repository exists and reviewers can access it.
- [ ] Both apps run and products load without CORS/JSON errors.
- [ ] API has the correct nested JSON and route.
- [ ] Caching and error handling have been checked.
- [ ] GitHub Actions is green.
- [ ] Genuine Copilot use and accepted changes are documented.
- [ ] REFLECTION.md placeholders are replaced with truthful observations.
- [ ] No bin/obj folders, credentials or build artifacts were committed.
