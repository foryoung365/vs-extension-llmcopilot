## Version Change Log

Versioning rules: v`Major.Minor.Build`

- Major: Incremented for significant program changes (e.g., from 1.0.0 to 2.0.0)
- Minor: Incremented for new features or optimizations (e.g., from 1.0.0 to 1.1.0)
- Build: Automatically incremented with each build, typically does not involve functional changes, and can be ignored.

### v1.5.x

- Added `Generate Unit Test` command.
- Added support for `vs2017`.
- Entering numbers will no longer trigger `code complete` to avoid incorrect prediction acceptance.

### v1.4.x

- Improved support for Dark Theme.
- Added support for using `Access Token` with reverse proxy. Leave blank if not needed.

### v1.3.x

- Optimized the issue where the prediction text box would sometimes appear continuously after rapid consecutive inputs.

### v1.2.x

- Optimized the chat window to allow input of newline characters and copying multiple lines of text into the chat box.
- Changed the key to send chat messages to `Ctrl+Enter`.
- Modified the key to accept all predictions to `Alt+Q`.
- Other minor optimizations.

### v1.1.x

- Fixed an issue where enabling the `code auto-completion` feature without opening the chat window would cause Visual Studio to crash during keyboard input.
- Fixed an issue where the prediction text box would overlap during continuous input.
- Optimized the code completion algorithm, making the response faster.
- Redesigned the prediction text box to make the distinction between line numbers and text more apparent.
