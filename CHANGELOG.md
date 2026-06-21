# Change log

## v1.1.1
**Fixes**
- Fixed an issue preventing installation via UPM.

## v1.1.0:
**Features**
- Added Debugger window. Allows the user to view and assign the main instance at any time.
- Added the option to destroy singletons at runtime only, editor only, or both. Defaults to both.

**Changes**
- Subclasses of a singleton will no longer show up in the projects settings menu.
- Added a debug message when a singleton is assigned as main.

**Fixes**
- Fixed an issue where null instances produced logs when attempting to be destroyed automatically.
- Fixed an issue where singleton GameObjects were left behind after the component was automatically destroyed.

---
## v1.0.0:

- Initial release