# Singletons

An implementation of the Singleton programming pattern for your Unity project.

A singleton is a type of object that acts as a single point of governance for game system, typically implemented using a script with a static field assigned on Awake. This singletons package provides a more robust solution with configurable behaviour for each type.
Options include:
- The ability to persist between scenes
- Newer instances taking priority over older ones
- Automatically destroy any other instances of it's type
- Automatically create a new instance if none was found

These options can be changed on a per-type basis, or inherit from a set of default settings.

Check out the GitHub page to contribute or report an issue:
https://github.com/JDoddsNAIT/Singletons