# About

Use the Singletons package to create objects that act as a single point of governance or "manager" of some feature in your game.
For example, Most projects use a `GameManager` script to manage things like player score, save data, loading scenes, etc. There should only ever be one active instance of this script at a time, and that instance must be accessible globally.
This package also features a tab in the project settings where the behaviour for each singleton type can be configured.


# Installation

Click [here](../README.md#installation) to view installation instructions.

# Usage
To create a singleton class, simply create a class that derives from Singleton<T>, as shown below:

```cs
using Singletons;

public class MyClass : Singleton<MyClass>
{
	// Script code goes here
}
```

Each singleton has a static reference to a single instance of its own type, called the Main Instance. Use the `GetInstance()` method to retrieve the current main instance.

You can configure the behaviour of singletons by going to `Edit > Project Settings > Singletons`.
All singleton types will appear in the settings after being added to any object in the scene. 
However if the script you're looking for does not appear in the settings, click the "Add Type" button and select the script type from the dropdown menu.

By default, a singleton will simply set itself as the main instance if there is not one already as soon as the script is loaded.
However, you can change this behaviour using the following options:

- **Persistent**: The main instance will persist between scenes.
- **Override Existing**: The most recently initialized object will be set as the main instance.
- **Destroy Others**: Instances that are not the main will be destroyed automatically.
- **Auto-Generation**: When the `GetInstance()` method is called and no main instance exists, a new GameObject will be created with the script attached. This new object becomes the main instance.

If Auto-Generation is enabled, you also have the option to create the singleton from a prefab instead of an empty GameObject.


# Technical Details

## Requirements
- Unity version 6.0 or greater.

This package depends on Unity's UI Toolkit for the project settings page.

## Revision History
|Date|Reason|
|---|---|
June 14th, 2026|Created document.