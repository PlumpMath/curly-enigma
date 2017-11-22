module Core.Main

open System

open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework.Input

open Component.Types
open Component.Functions
open Managers
open EntityGenerator
open Game
open Scenes

type GameRoot () as gr =
    inherit Game()

    do gr.Content.RootDirectory <- "content"

    let graphics = new GraphicsDeviceManager(gr)
    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>

    let mutable gameData =
        [
            npc <| simpleEntity ((50,50), (20.0f,20.0f), (20, 20), (120, 200, 10, 255));
            player <| Speed 1.0f <| Duration 300.0f <| movingEntity ((100, 100), (50.0f,50.0f), (0.0f,0.0f), "dot", (0,0,255,255));
            npc <| texturedEntity ((300, 200), (50.0f,50.0f), "dot", (0,100,100,255));
            npc <| movingEntity ((200,200), (50.0f,50.0f), (0.5f,0.0f), "dot", (0,10,130,255));
        ]
        |> List.mapi initalizeEntities
        |> List.collect id
        |> List.unzip
        |> buildGameData
    
    let mutable textureMap =
        Map.empty<TextureId,Texture2D>
    
    let mutable effectMap =
        Map.empty<EffectId,Effect>
    
    override gr.Initialize() =
        do spriteBatch <- new SpriteBatch(gr.GraphicsDevice)
        do base.Initialize()
        ()
    
    override gr.LoadContent() =
        let components =
            match gameData.GameState with
            | Playing data -> data.Components
            | Done -> [] |> buildComponentSystem
        textureMap <-
            components
            |> TextureManager.loadTextures gr.Content
        effectMap <-
            EffectManager.loadEffects gr.Content
        ()

    override gr.Update (gameTime) =
        let currentKeyboardState = Keyboard.GetState()

        let state' = 
            match gameData.GameState with
            | Playing data ->
                PlayingScene.update gameTime currentKeyboardState gameData data
            | Done -> Done

        gameData <- 
            { gameData with
                GameState = state';
                PreviousKeyboardState = Some currentKeyboardState;
            }
        ()
    
    override gr.Draw (gameTime) =
        match gameData.GameState with
        | Playing data ->
            PlayingScene.draw graphics.GraphicsDevice textureMap spriteBatch data
            ()
        | Done -> ()