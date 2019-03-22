using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using OpenGL_Game.Systems;
using OpenGL_Game.Managers;
using OpenGL_Game.Scenes;
using OpenGL_Game.Components;
using System.Drawing;
using System;
using OpenGL_Game.Objects;
using System.Collections.Generic;
using System.Threading;
using OpenTK.Audio;

namespace Game_Demo
{
    /// <summary>
    /// Initial game scene for testing
    /// </summary>
    public class GameScene : Scene
    {

        private int NumberOfGhost, NumberOfLives, NumberOfCoins, PlayerScore, CoinsCollected;
        private Vector3 StartPosition_Player, StartPosition_Ghost;
        private bool isPowerUp, isPowerUpSound;
        private float PowerUpTimer;
        // Debug Demo related Variables
        private bool DebugGhosts = false, DebugCollision = false;
        string[] DebugCollidables;
        // Audio features
        AudioContext audioContext = new AudioContext();
        List<Audio> SoundEffects = new List<Audio>();

        public GameScene(SceneManager sceneManager, SystemManager systemManager, EntityManager entityManager) : base(sceneManager, systemManager, entityManager)
        {
            // Set the title of the window
            sceneManager.Title = "Game";
            sceneManager.Icon = Icon.ExtractAssociatedIcon("Textures/pacman.ico");
            // Set the Render and Update delegates to the Update and Render methods of this class
            sceneManager.renderer = Render;
            sceneManager.updater = Update;
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            // Updates SceneManager that this scene is active
            sceneManager.ShowCursor = false;
            sceneManager.MouseControlledCamera = true;
            // Set starting position vectors
            StartPosition_Player = new Vector3(12.0f, 1.3f, 20.0f);
            StartPosition_Ghost = new Vector3(0.0f, 0.0f, -1.0f);
            // Sets up players camera
            sceneManager.camera.Position = StartPosition_Player;
            sceneManager.camera.Rotate(0.0f, -1.0f, 0.0f, 0.1f);

            // Creates basic scene functionality
            CreateEntities();
            CreateSystems();
            LoadAllSoundEffect();
            // Game value intialisation
            NumberOfGhost = entityManager.FindEntityWithMask(ComponentTypes.COMPONENT_AI).Length;
            NumberOfLives = 3;
            NumberOfCoins = 66;
            CoinsCollected = 0;
            PlayerScore = 0;
            isPowerUp = false;
            isPowerUpSound = false;
            PowerUpTimer = 0.0f;
        }
        /// <summary>
        /// Loads all entities for this scene from file(s) specified
        /// </summary>
        private void CreateEntities()
        {
            // Loads in all basic static map entities
            entityManager.LoadSceneEntities("Entities/static_GameScene.xml");
            // Loads in all dynamic entities
            entityManager.LoadSceneEntities("Entities/dynamic_GameScene.xml");
            // Loads in all collectable entities
            entityManager.LoadSceneEntities("Entities/collectable_GameScene.xml");
        }
        /// <summary>
        /// Intilises all systems required for this scene and updates SystemManager
        /// </summary>
        private void CreateSystems()
        {
            // Intialises all systems needed in this scene and add them to SystemManager
            ISystem newSystem;
            // Creates the system to calculate the SkyBox
            newSystem = new SystemSkyBox(ref sceneManager.camera);
            systemManager.AddSystem(newSystem);

            // Creates an array of light point for use in SystemRenderer
            Vector3[] array = new Vector3[]{
                new Vector3(0.0f, 1.0f, 0.0f),
                new Vector3(17.0f, 1.0f, 15.0f),
                new Vector3(17.0f, 1.0f, -15.0f),
                new Vector3(-17.0f, 1.0f, 15.0f),
                new Vector3(-17.0f, 1.0f, -15.0f) };
            // Creates the system to calculate all the rendering (including lighting)
            newSystem = new SystemRender(ref sceneManager.camera, array);
            systemManager.AddSystem(newSystem);
            // Creates the system to calculate all the collision (and trigger) events
            newSystem = new SystemCollider(ref entityManager, ref sceneManager.camera);
            systemManager.AddSystem(newSystem);
            // Creates the system to calculate all the AI paths and behaviours
            newSystem = new SystemAI(50, 50, entityManager.Entities().ToArray());
            systemManager.AddSystem(newSystem);
            // Creates the system to update all the audio effects
            newSystem = new SystemAudio();
            systemManager.AddSystem(newSystem);
            // Creates the system to calculate all the animation transformions
            newSystem = new SystemAnimator();
            systemManager.AddSystem(newSystem);

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="e">Provides a snapshot of timing values.</param>
        public override void Update(FrameEventArgs e)
        {
            // Checks if the player has collected all the coins needed to win the level
            if(CoinsCollected >= NumberOfCoins) { sceneManager.ChangeScene(3); return; }
            // Game timing related features
            float dt = (float)e.Time;
            if(PowerUpTimer > 0)
            {
                // Manages when the Power up sound effect plays and for how long and also updates playing position
                PowerUpTimer -= dt;
                SoundEffects[4].UpdateEmitterPosition(sceneManager.camera.Position);
                // This will only run once right after the power up is collected to prevent multiple sound effects playing
                if (!isPowerUpSound)
                {
                    // Updates the sound effects settings
                    SoundEffects[4].VolumeOfAudio = 0.4f;
                    SoundEffects[4].LoopPlayAudio = true;
                    SoundEffects[4].PlayAudio();
                    isPowerUpSound = true;
                }
                // If this passes then the power up period has finised and all returns to normal
                if(PowerUpTimer <= 0)
                {
                    PowerUpTimer = 0.0f;
                    isPowerUp = false;
                    isPowerUpSound = false;
                    // Make sure to clean up audio trail
                    SoundEffects[4].Close();
                }
            }

            // This is required for the AI (and physics) system to work as the AI require deltatime to work consitently
            SystemAI AI = (SystemAI)systemManager.FindSystem("SystemAI");
            if (DebugGhosts) { AI.UpdateDeltaTime(0.0f); } else { AI.UpdateDeltaTime(dt); }

            // This is requried for consitent animation across systems
            SystemAnimator Animator = (SystemAnimator)systemManager.FindSystem("SystemAnimator");
            Animator.UpdateDeltaTime(dt);
            // Updates the AI to follow the player when its position changes
            ManageGhostTargets();
            // Checks for trigger collisions that need acting on such as item pickup or ghost collision
            ManageTriggerCollisions();
            // Handles the player inputs to move the player
            ManageMovement(dt);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="e">Provides a snapshot of timing values.</param>
        public override void Render(FrameEventArgs e)
        {
            // Game Rendering 
            systemManager.ActionSystems(entityManager);

            // HUD Rendering 
            // Sets the background of the HUD to clear so the 3D enviroment can still be seen
            GUI.clearColour = Color.Transparent;

            // Sets basic canvas size variables
            float width = sceneManager.Width, height = sceneManager.Height, fontSize = Math.Min(width, height) / 30f;
            // Displays a score label in the top left of the screen and updates based on the players score (times 10)
            GUI.Label(new Rectangle((int)(-width / 2) + 128, (int)(fontSize / 2f), (int)width, (int)(fontSize * 2f)), "Score: " + (PlayerScore * 10).ToString(), (int)fontSize, StringAlignment.Center);
            // For each life a image is added to reperesent it
            // Called on each render frame so if life is lost it 'updates' the display for lives remaining automatically
            if (NumberOfLives < 0) { NumberOfLives = 0; }
            for (int i = 0; i < NumberOfLives; i++)
            {
                int offset = 96 * i;
                // Adds the image to the bottom left corner with a space betwen each
                GUI.DrawImage((int)(width * 0.03f) + offset, (int)(height * 0.85f), "Textures/pacman.png");
            }
            // Renders the above GUI items to the screen
            GUI.Render();
        }
        /// <summary>
        /// Method to manage all of the player inputs and to act on them
        /// </summary>
        /// <param name="dt">delta time</param>
        private void ManageMovement(float dt)
        {
            // Checks if the player want to exit the game and takes them to the main menu
            if (GamePad.GetState(1).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Key.Escape))
            {
                sceneManager.ChangeScene(0);
                return;
            }
            float Mspeed = 6.0f, Rspeed = 4.0f;
            // Handles the players movement forwards
            if (Keyboard.GetState().IsKeyDown(Key.Up) || Keyboard.GetState().IsKeyDown(Key.W))
            { sceneManager.camera.Move(new Vector3(0.0f, 0.0f, 1.0f), Mspeed * dt); }
            // Handles the players movement backwards
            if (Keyboard.GetState().IsKeyDown(Key.Down) || Keyboard.GetState().IsKeyDown(Key.S))
            { sceneManager.camera.Move(new Vector3(0.0f, 0.0f, -1.0f), Mspeed * dt); }
            // Handles the players movement left
            if (Keyboard.GetState().IsKeyDown(Key.Left) || Keyboard.GetState().IsKeyDown(Key.A))
            { sceneManager.camera.Rotate(1.0f, 0.0f, 0.0f, Rspeed * dt); }
            // Handles the players movement right
            if (Keyboard.GetState().IsKeyDown(Key.Right) || Keyboard.GetState().IsKeyDown(Key.D))
            { sceneManager.camera.Rotate(-1.0f, 0.0f, 0.0f, Rspeed * dt); }

            // Debug controls to meet ACW Specification
            // Powerful machines will hit this method multiple times while the key is pressed so 
            // it may flick between debug options being true and false meaning it might not work first time
            if (Keyboard.GetState().IsKeyDown(Key.G))
            {
                // Toggles if the ghost AI is enabled or not
                if (DebugGhosts) { DebugGhosts = false; } else { DebugGhosts = true; }
            }
            // Toggles the collsion of all the rigid collidable entities
            if (Keyboard.GetState().IsKeyDown(Key.C))
            {
                if (DebugCollision)
                {
                    // When re enabled the list of turned of colliders are set to true again
                    DebugCollision = false;
                    for (int i = 0; i < DebugCollidables.Length; i++)
                    {
                        ComponentCollider c =  (ComponentCollider)entityManager.FindEntity(DebugCollidables[i]).FindComponent(ComponentTypes.COMPONENT_COLLIDER);
                        c.isCollidable = true;
                    }
                }
                else
                {
                    // To keep track of which entites are effected a list of their names (all unique) are added to a list
                    // for use the in the code above
                    List<string> names = new List<string>();
                    DebugCollision = true;
                    Entity[] entities = entityManager.FindEntityWithMask(ComponentTypes.COMPONENT_COLLIDER);
                    for (int i = 0; i < entities.Length; i++)
                    {
                        ComponentCollider c = (ComponentCollider)entities[i].FindComponent(ComponentTypes.COMPONENT_COLLIDER);
                        if (c.isCollidable == true)
                        {
                            names.Add(entities[i].Name);
                            c.isCollidable = false;
                        }
                    }
                    DebugCollidables = names.ToArray();

                }
            }
        }
        /// <summary>
        /// Method to organise the ghost AI target points
        /// </summary>
        private void ManageGhostTargets()
        {
            // For every ghost in the system AI update its target
            for (int i = 0; i < NumberOfGhost; i++)
            {
                // Gets access to the AI componet and position component to update their values for each AI 
                ComponentAI Ai = (ComponentAI)entityManager.FindEntityWithMask(ComponentTypes.COMPONENT_AI)[i].FindComponent(ComponentTypes.COMPONENT_AI);
                ComponentPosition Pos = (ComponentPosition)entityManager.FindEntityWithMask(ComponentTypes.COMPONENT_POSITION)[i].FindComponent(ComponentTypes.COMPONENT_POSITION);
                // If the player is powered up the the target should be the ghost start (makes them run away from the player)
                if (isPowerUp)
                { Ai.Target = StartPosition_Ghost; }
                // If the player is not powered up then the target is the player (the ghosts be hungery)
                else
                { Ai.Target = RoundVector(sceneManager.camera.Position); }
            }
        }
        /// <summary>
        /// Method to organise the trigger collsion detection
        /// </summary>
        private void ManageTriggerCollisions()
        {
            // Get access to the collision system to find out which entity (if any) have caused a trigger
            // (Trigger describes entity with collision that is not rigid e.g. pickup item compared to a wall)
            SystemCollider collide = (SystemCollider)systemManager.FindSystem("SystemCollider");
            // Checks to make sure checks only run if a trigger has occured
            if (collide.GetEntityCollisionTrigger() != null && collide.GetEntityCollisionTrigger() != "")
            {
                // First check is to see if the player has collided with any of the ghosts
                // Works by developing the entities xml file with consitent and sensible names per entity
                if (collide.GetEntityCollisionTrigger().Contains("Ghost"))
                {
                    // If the player picked up a power up within the last 10seconds then the player damages the ghost
                    // Otherwise the ghost damages the player, removing a life and reseting both player and ghosts positions to the starting points
                    if (isPowerUp)
                    {
                        // Updates sound effects settings and plays sound effect
                        SoundEffects[3].UpdateEmitterPosition(sceneManager.camera.Position);
                        SoundEffects[3].VolumeOfAudio = 1.0f;
                        SoundEffects[3].PlayAudio();
                        // Sets the positon of the ghost that was hit to the starting position
                        ComponentPosition Pos = (ComponentPosition)entityManager.FindEntity(collide.GetEntityCollisionTrigger()).FindComponent(ComponentTypes.COMPONENT_POSITION);
                        Pos.Position = StartPosition_Ghost;
                        // Updates the players score
                        PlayerScore += 10;
                    }
                    else
                    {
                        // Player has been hit by a ghost so a life is taken off and players position set back to the starting positon
                        NumberOfLives--;
                        sceneManager.camera.Position = StartPosition_Player;
                        // This could be improved, the collision detection keeps track of players last position
                        // If it isn't updated the after the player is moved to the starting point they will be moved straight back again
                        collide.UpdateLastPosition(StartPosition_Player);
                        // Plays sound effect for player getting hit by a ghost and updates it settings
                        SoundEffects[1].UpdateEmitterPosition(sceneManager.camera.Position);
                        SoundEffects[1].VolumeOfAudio = 20.0f;
                        SoundEffects[1].PlayAudio();
                        // All ghosts will have their positions reset to the starting position
                        for (int i = 0; i < NumberOfGhost; i++)
                        {
                            ComponentPosition Pos = (ComponentPosition)entityManager.FindEntityWithMask(ComponentTypes.COMPONENT_AI)[i].FindComponent(ComponentTypes.COMPONENT_POSITION);
                            Pos.Position = StartPosition_Ghost;
                        }
                        // Finally checks if the player has run out of lives and if so it moves to the game over scene
                        if (NumberOfLives <= 0)
                        {
                            sceneManager.NextScene();
                        }
                    }
                    // Clears the last trigger so it doesn't get stuck in an infinite loop
                    collide.ClearLastTrigger();
                }
                // Checks if the player has collided with one of the coins
                // Again making use of using consitent and sensible names in the entities xml files
                else if (collide.GetEntityCollisionTrigger().Contains("coin"))
                {
                    // Plays the pick up sound for a coin and updates the sound effects settings
                    SoundEffects[0].UpdateEmitterPosition(sceneManager.camera.Position);
                    SoundEffects[0].VolumeOfAudio = 0.2f;
                    SoundEffects[0].PlayAudio();
                    // Coin has been hit so it is now removed from the entity list so it wont be managed anymore in this instance of this game scene
                    entityManager.RemoveEntity(collide.GetEntityCollisionTrigger());
                    // Clears pickable collision check
                    collide.ClearLastPickable();
                    // Updates the players score and how many coins their have collected
                    // These are seperate as the players score can be added to by things outside of pickups such as killing a ghost
                    PlayerScore++;
                    CoinsCollected++;
                }
                // Checks if the player has collcied with one of the powerup items
                // Again making use of using consitent and sensible names in the entities xml files
                else if (collide.GetEntityCollisionTrigger().Contains("power"))
                {
                    // Sets the bool for is the player powered up or not to true
                    isPowerUp = true;
                    // Sets the value for how long the powerup is to last 1.0f = 1 second
                    PowerUpTimer = 10.0f;
                    // Plays the sound effect for the power up being picked up
                    SoundEffects[2].UpdateEmitterPosition(sceneManager.camera.Position);
                    SoundEffects[2].VolumeOfAudio = 0.4f;
                    SoundEffects[2].PlayAudio();
                    // Just like the coin it has been collected so needs to be removed for entity list so its no longer managed
                    entityManager.RemoveEntity(collide.GetEntityCollisionTrigger());
                    collide.ClearLastPickable();
                    // Updates the players score and items collected
                    PlayerScore += 5;
                    CoinsCollected++;
                }
            }
        }
        /// <summary>
        /// Loads in all sound effects that are not entity bound.
        /// </summary>
        void LoadAllSoundEffect()
        {
            // Makes sure to still load through ResourceManager
            // ID: 0 - Coin pick up sound effect
            SoundEffects.Add(new Audio(ResourceManager.LoadAudio("Audio/sfx_coin_single5.wav")));
            // ID: 1 - Ghost damage player sound effect
            SoundEffects.Add(new Audio(ResourceManager.LoadAudio("Audio/sfx_wpn_laser.wav")));
            // ID: 2 - Power up pick up sound effect
            SoundEffects.Add(new Audio(ResourceManager.LoadAudio("Audio/sfx_sounds_powerup9.wav")));
            // ID: 3 - Player damage ghost sound effect
            SoundEffects.Add(new Audio(ResourceManager.LoadAudio("Audio/sfx_sounds_powerup4.wav")));
            // ID: 4 - Powerup timer sound effect
            SoundEffects.Add(new Audio(ResourceManager.LoadAudio("Audio/sfx_alarm_loop6.wav")));
        }
        /// <summary>
        /// Basic method used to round vectors within this class
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        Vector3 RoundVector(Vector3 vec)
        { return new Vector3((float)Math.Round(vec.X), vec.Y, (float)Math.Round(vec.Z)); }

        /// <summary>
        /// This is called when the game exits.
        /// </summary>
        public override void Close()
        {
            // Cleans up all none entity bound audio
            for (int i = 0; i < SoundEffects.Count; i++)
            {
                SoundEffects[i].Close();
            }
            // Closes the audio context
            audioContext.Dispose();
        }
    }
}
