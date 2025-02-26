# 🛠️ Contributing to PropertyBitPack

We appreciate your interest in contributing to **PropertyBitPack**! 🚀  
To keep the process smooth, please follow these guidelines.

---

## 🚀 How to Contribute

### 🔹 Reporting Issues
- **Check existing issues** first: [Issues](https://github.com/Asaicraft/PropertyBitPack/issues).
- Use the **issue templates** when creating new issues.
- Provide **clear descriptions and code samples** if applicable.

### 🔹 Feature Requests
- Before submitting a feature request, **discuss it first** in:
  - [GitHub Discussions (Ideas Category)](https://github.com/Asaicraft/PropertyBitPack/discussions/categories/ideas)
  - [Discord](https://discord.gg/RpxD2BeNsZ)

---

## 📌 Commit Message Rules

We follow the **Conventional Commits** standard to maintain consistency and readability in commit history.

### 🔹 **Commit Message Format**
```text
<type>: <short summary>
```
Where:
- `<type>` — the type of change (see below).
- `<short summary>` — a brief description of the change (starting with an **imperative verb**, without a period).

### ✅ **Allowed Commit Types**
| Type       | Description                                      |
|------------|--------------------------------------------------|
| `feat`     | Adds new functionality                           |
| `fix`      | Fixes a bug                                      |
| `refactor` | Improves code without changing behavior          |
| `docs`     | Updates documentation                            |
| `test`     | Adds or updates tests                            |
| `chore`    | Technical tasks (dependencies, CI/CD config)     |
| `ci`       | Changes to CI/CD configuration                   |
| `style`    | Formatting, whitespace, etc. (no logic changes)  |
| `perf`     | Performance optimization                         |

### 🔹 **Commit Message Examples**
```text
feat: add support for bitwise operations
fix: resolve incorrect bit-packing issue
refactor: improve property generator logic
docs: update README with installation guide
```

---

## ⚠️ **Important Note About `docs` and `chore` Commits**
The **CI/CD pipeline will not run** for commits that start with `docs` or `chore`.  

🔴 **Avoid using these types unless necessary.**  
✅ Instead, use more specific types such as `feat`, `fix`, or `refactor`.  

If your commit is purely technical, use **clear descriptions** in the commit body.

---

## 🔹 Pull Requests

1. **Fork the repository** and create a new branch (`feature/your-feature`).
2. **Follow our coding style** (see below).
3. **Write unit tests** for new features when possible.
4. **Run `dotnet format`** before committing.
5. **Ensure CI checks pass** before submitting your PR.

Once your PR is ready:
- **Write a clear PR description**.
- **Link related issues** using `Fixes #issue_number`.

---

## 🎨 Code Style & Best Practices
- Use **PascalCase** for class names and **camelCase** for variables.
- Write **XML documentation** for public methods.
- Keep code **clean and readable**.
- Format code using:
  ```sh
  dotnet format
  ```

### ✅ **Checklist for PRs**
- [ ] PR title follows commit message guidelines.
- [ ] Code is formatted correctly (`dotnet format`).
- [ ] Tests are included if necessary.
- [ ] CI checks pass before merging.

---

## 🔐 Licensing & Code Authenticity

### 🔹 **Code Must Be Your Own**
By contributing to PropertyBitPack, you agree that:
- All code you submit is **written by you** or **properly licensed**.
- You **do not** submit code copied from other projects **without explicit permission** and proper attribution.
- If your code is based on someone else's work, you must **clearly state this in the PR description**.

### 🔹 **License Compliance**
PropertyBitPack follows the **MIT License**, which means:
- Your contributions will be licensed under the same terms.
- You agree that your code **can be used, modified, and redistributed** under the MIT License.
- You **must not submit proprietary or confidential code**.

If you are unsure whether your contribution complies with these rules, **please ask in [Discussions](https://github.com/Asaicraft/PropertyBitPack/discussions) before submitting a PR.**

---

## 📜 Code of Conduct
This project follows the **Contributor Covenant Code of Conduct**.  
Please read it [here](https://github.com/Asaicraft/PropertyBitPack/blob/master/CODE_OF_CONDUCT.md).

## 💬 Need Help?
- **Ask in [Discussions](https://github.com/Asaicraft/PropertyBitPack/discussions)**.
- **Join our [Discord](https://discord.gg/RpxD2BeNsZ)**.

Thank you for contributing! 🎉