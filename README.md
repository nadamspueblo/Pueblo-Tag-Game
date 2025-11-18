# Pueblo Tag
## 🎮 Student Collaboration Checklist

Follow these steps every time you work on the project. This workflow uses the Source Control features built right into VS Code to keep our project conflict-free!

1. Start Your Session
    - [ ] **Open the Project Folder** in VS Code.
    - [ ] Click the **Source Control** icon (branch icon) on the left sidebar.
    - [ ] **Fetch & Pull**: 
      - Click the **... (More Actions)** menu (three dots) in the Source Control panel title bar.
      - Select **Pull** to get the latest updates from everyone else.
    - [ ] **Create a New Branch**: 
      - Click on the current branch name in the bottom-left corner of the window (it likely says ```main```).
      - Select **+ Create new branch...** from the menu that pops up at the top.
      - *Naming Format:* ```yourname-task``` (e.g., ```dario-mainbuilding```).

2. Open the Scene in Unity
    - [ ] Open Unity Hub and launch the project.
    - [ ] In the **Project** window, navigate to ```Assets/Scenes/```.
    - [ ] Open the main ```GameScene```.

3. 🛑 CRITICAL: "Check Out" Your Slide  
*You must tell Unity which specific file you are editing so you don't overwrite the main game file.*
    - [ ] In the Hierarchy, find your assigned SubScene (e.g., ```Level_MainBuilding```, ```Level_Terrain```).
    - [ ] Tick the Checkbox next to your SubScene's name. The tooltip should say "Toggle whether the Sub Scene is open for editing".
    - [ ] Verify: Ensure the checkbox is OFF for GameScene and all other student scenes. Only your scene should be checked.

4. Build & Create
    - [ ] Perform your modeling, texturing, or placement work.
    - [ ] Prefab Rule: If you create a new object (like a chair or tree), drag it into the Assets/Prefabs/ folder to make it a Prefab.
    - [ ] Reference Check: You can look at other students' work in the Hierarchy to line things up, but do not move or edit their objects.

5. Save & Verify (The "Safety Check")
    - [ ] Go to File > Save Project in Unity.
    - [ ] Switch back to VS Code and look at the Source Control tab.
    - [ ] VERIFY the "Changes" list:
      - ✅ You SHOULD see: Assets/Scenes/GameScene/Level_YourName.unity
      - ✅ You SHOULD see: Any new Prefabs or Materials you created.
      - ❌ You should **NOT** see: Assets/Scenes/GameScene.unity

    - [ ] *Troubleshooting*: If ```GameScene.unity``` is in the list, hover over it and click the Discard Changes arrow icon (make sure you haven't accidentally added objects to the wrong scene first!).

6. Submit Work
    - [ ] In the **Message** box (above the "Commit" button), write a clear summary (e.g., "Added windows to cafeteria").
    - [ ] Click **Commit**.
    - [ ] Click **Publish Branch** (or **Sync Changes**).
    - [ ] Go to the GitHub website for our repository and create a **Pull Request** merging your branch into ```main```.

## TO-DO

### Create Building Models
1. Use ProBuilder tools to create the basic structure of buildings
2. Add textures and details
3. Find 3D assets for tables, desks, chairs, etc.

### Create textures
1. Research creating textures
1. Take photos for textures around campus
  - Start with terrain textures
2. Create normal maps textures: [NormalMap Online](https://cpetry.github.io/NormalMap-Online/)
3. Create textures in Unity

### Build terrain
1. Research using Terrain in Unity
1. Apply textures
2. Build topography
3. Find 3D assets for trees, grass, weeds, stones, litter, etc.
4. Paint trees and details

### Gameplay
1. Spawn points
2. Find 3D character assets for the players/NPCs
3. Create NPCs
4. Find 3D assets for power-ups
