# Unity Editor Tools Collection

A comprehensive collection of professional Unity Editor tools designed to streamline development workflow, improve productivity, and simplify common tasks in Unity projects.

## 🚀 Features

This repository contains 6 powerful editor tools:

1. **Component Finder** - Advanced scene and project-wide component search
2. **Localization Sync Tool** - Google Sheets integration for multi-language management
3. **Light Batch Bake Tool** - Bulk lighting configuration and optimization
4. **Mesh Renderer Lighting Tool** - Material and lighting property management
5. **Terrain Duplication Tool** - Smart terrain cloning with unique data
6. **Localization Setup Helper** - Interactive guide for Google Apps Script setup

---

## 📦 Installation

1. Clone or download this repository
2. Copy the `.cs` files into your Unity project's `Assets/Editor/` folder
3. Unity will automatically compile the scripts
4. Access tools from the Unity menu bar under `Tools/`

```bash
git clone https://github.com/Halilibrahimeris/Unity-Tools.git
```

---

## 🛠️ Tools Documentation

### 1. Component Finder Window

**Menu Path:** `Tools → Component Finder`

A powerful search tool to locate GameObjects containing specific components across your entire project.

#### Features:
- Search in open scenes, active scene only, or project prefabs
- Filter by specific folder paths
- Visual results list with quick selection and ping
- Batch operations (Select All, Ping All)
- Supports all Component types including custom scripts

#### Usage:
1. Drag a MonoBehaviour script into the "Script" field
2. Choose search scope (Open Scenes, Active Scene, Project Prefabs, Selected Folder)
3. Click "Find" to locate all instances
4. Use Select/Ping buttons to navigate to found objects

#### Example Use Cases:
- Find all objects using a deprecated component
- Locate specific enemy AI instances across multiple scenes
- Audit prefab usage throughout the project

---

### 2. Localization Sync Tool

**Menu Path:** `Tools → Localization Sync → Web App Sync`

Synchronize Unity's Localization package with Google Sheets for collaborative translation workflows.

#### Features:
- **No DLL Dependencies** - Uses built-in Unity networking
- Bidirectional sync (Unity ↔ Google Sheets)
- Support for multiple languages/locales
- Automatic locale detection and mapping
- Real-time status feedback panel
- Persistent URL storage per project

#### Setup:
1. Click the Help (?) button in the tool window
2. Follow the interactive guide to:
   - Create a Google Sheet
   - Deploy Apps Script web app
   - Copy the deployment URL
3. Paste URL into the tool and select your StringTableCollection

#### Operations:

**Push (Unity → Sheet):**
```
Exports all localization keys and translations to Google Sheets
Structure: Key | ID | Language1 | Language2 | ...
```

**Pull (Sheet → Unity):**
```
Imports translations from Google Sheets back into Unity
Automatically creates missing entries and updates existing ones
```

#### Benefits:
- Translators can work directly in Google Sheets
- No need for Unity knowledge
- Version control friendly
- Team collaboration support
- Real-time translation updates

---

### 3. Light Batch Bake Tool

**Menu Path:** `Tools → Lighting → Light Bake Tool`

Bulk configure and optimize lighting settings across your entire scene.

#### Features:
- **Batch Bake Type Configuration** (Realtime/Baked/Mixed)
- **Bulk Range Adjustment** for Point and Spot lights
- **Bulk Intensity Control** for all light types
- Individual light property editing
- Per-light bake type override
- Scene organization (sorted by scene name)
- Undo/Redo support
- Auto scene dirty marking

#### Interface:
- Checkboxes for selective operations
- Real-time property editing in scrollable list
- Quick access buttons (Refresh, Select All, Deselect All)
- Scene and light name display
- Ping functionality for quick navigation

#### Workflow:
1. Open the tool to see all lights in loaded scenes
2. Select lights using checkboxes
3. Choose bulk settings (Bake Type, Range, Intensity)
4. Apply to selected lights or all lights
5. Fine-tune individual lights in the list

#### Performance Tips:
- Excludes Area lights (Rectangle/Disc) automatically
- Displays light count for performance tracking
- Directional lights excluded from range operations

---

### 4. Mesh Renderer Lighting Tool

**Menu Path:** *Custom implementation - check your menu structure*

Advanced tool for managing MeshRenderer lighting properties and material emission.

#### Features:
- Material emission control
- Lighting property batch operations
- Support for multiple renderers
- Real-time preview
- Undo/Redo integration

#### Use Cases:
- Configure emission for interactive objects
- Batch update lighting properties
- Optimize material settings for performance

---

### 5. Duplicate Terrain with Unique Data

**Menu Path:** `Tools → Duplicate Terrain (Unique Data)`

Smart terrain duplication that creates independent TerrainData assets.

#### Features:
- Creates unique TerrainData asset copy
- Automatic asset organization (`Assets/ClonedTerrains/`)
- Timestamped file naming
- Preserves all terrain properties:
  - Height maps
  - Texture alpha maps
  - Detail layers
  - Tree instances
- Automatic offset positioning
- Selection of cloned terrain

#### Usage:
1. Select a Terrain GameObject in the scene
2. Run `Tools → Duplicate Terrain (Unique Data)`
3. New terrain appears offset to the right with unique data

#### Benefits:
- Avoids shared TerrainData references
- Safe for independent editing
- Preserves original terrain integrity
- Ideal for creating terrain variations

---

### 6. Localization Help Window

**Menu Path:** Opens automatically via Localization Sync Tool's (?) button

Interactive setup guide for Google Apps Script integration.

#### Features:
- **Bilingual Interface** (English & Turkish)
- Step-by-step setup instructions
- Embedded Apps Script code
- One-click code copying
- Direct Google Sheets link
- Scrollable code viewer

#### Apps Script Features:
- **doGet()** - Fetches spreadsheet data as JSON
- **doPost()** - Receives Unity data and updates sheet
- Error handling and status reporting
- Automatic sheet clearing and updating

#### Setup Flow:
1. Create Google Sheet
2. Open Apps Script editor
3. Copy provided code
4. Deploy as web app
5. Set access to "Anyone"
6. Copy deployment URL

---

## 🔧 Technical Requirements

- **Unity Version:** 2020.3 or higher recommended
- **Dependencies:**
  - Unity Localization Package (for sync tools)
  - Unity EditorCoroutines (for sync tools)
  - Newtonsoft.Json (included in modern Unity versions)

---

## 📋 Best Practices

### Component Finder
- Use folder filtering for large projects to improve performance
- Search prefabs separately to avoid long loading times

### Localization Sync
- Always backup your StringTableCollection before pulling
- Test with a small table first
- Ensure Google Apps Script is set to "Anyone" access
- Keep the deployment URL secure if data is sensitive

### Light Bake Tool
- Use "Apply to Selected" for testing settings
- Enable bulk options only when needed
- Check DirectionalLight separately (no range)
- Save scene after bulk operations

### Terrain Duplication
- Clone TerrainData is saved in `Assets/ClonedTerrains/`
- Clean up old clones periodically
- Use timestamped names for organization

---

## 🐛 Troubleshooting

### Localization Sync Issues

**Error: "Connection Error"**
- Verify Google Apps Script deployment URL
- Check "Who has access" is set to "Anyone"
- Ensure Apps Script code is deployed (not just saved)

**Error: "No locale columns matched"**
- Check Google Sheet header format: `DisplayName (code)`
- Verify locale codes match Unity's Localization settings
- Example: `English (en)`, `Turkish (tr)`

### Component Finder Issues

**"Type could not be retrieved"**
- Script may have compilation errors
- Ensure the script is a valid Component class
- Check script is not abstract or generic

### Light Bake Tool Issues

**Lights not updating**
- Ensure scene is saved
- Check undo history isn't full
- Verify lights aren't on locked layers

---

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Code Standards:
- Follow Unity C# coding conventions
- Add XML documentation for public methods
- Include error handling and user feedback
- Test in multiple Unity versions if possible

---

## 🙏 Acknowledgments

- Built for the Unity game development community
- Inspired by common workflow pain points
- Designed for both solo developers and teams

---

## 📞 Support

If you encounter issues or have suggestions:

1. Check the Troubleshooting section above
2. Open an issue on GitHub
3. Include Unity version and error messages
4. Provide steps to reproduce

---

## 🗺️ Roadmap

Planned features and improvements:

- [ ] Component Finder: Add component replacement tool
- [ ] Localization: Support for CSV export/import
- [ ] Light Tool: Add preset save/load functionality
- [ ] Terrain: Multi-terrain batch operations
- [ ] General: Add preference saving for all tools

---

## 📊 Version History

### v1.0.0 (Current)
- Initial release
- 6 core editor tools
- Turkish language support
