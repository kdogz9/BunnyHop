# Bunny Hop Dev log 


## Planning (73)
Firstly, I thought about procedural generation and what I wanted to gain from it. I decided to explore procedural audio generation and turn it into a rhythm game. I wanted the music to be the main feature rather than something that usually plays in the background of games. I watched YouTube tutorials to help guide me and used games such as Geometry Dash (Geometry Dash - Official Website, s.d.) as inspiration to understand how rhythm-based gameplay could engage players.

## Development (232)
I started by following a YouTube tutorial (Making Procedural Music in Unity, 2022) that explained how procedural audio generation works. Using this as a guide, I created a script called Procedural Audio Generator. This converts BPM into time intervals between beats and uses Random.Range to randomly select notes from a musical scale. It then generates waveform samples directly in RAM and plays them. The system continues generating notes, creating a procedural audio sequence. I chose to generate the audio samples in RAM rather than relying on prerecorded audio files, allowing the audio to be created at runtime for WebGL. 

```
beatInterval = 60f / bpm;

// Converts BPM (Beats Per Minute) into exact time intervals between beats

int noteIndex = UnityEngine.Random.Range(0, currentScale.Length);
float frequency = currentScale[noteIndex];

// Uses Random.Range to randomly select a note frequency from the active musical scale
```

I then created a script to manage player input and game logic. This controls the bunny's hopping when the player presses the spacebar. It calculates the time difference between the player's input and the latest beat. If the input falls within the allowed timing window, it registers as a successful hop. 

```

    if (Input.GetKeyDown(KeyCode.Space))
    // Listens for Spacebar input to control the bunny's hopping
    {
       
        float timeDiff = Mathf.Abs(Time.time - lastBeatTime);
         // Calculates the time difference between player input and the latest beat tick

       
        if (timeDiff <= timingWindow)
        {
            RegisterSuccessfulHop();
             // If input falls within the allowed timing window, register a successful hop
        }
        else
        {
            RegisterMissedHop();
        }
```

The script also manages the scoring system. Collecting carrots increases the score, while missing beats or colliding with obstacles deducts points.

```
public void AddCarrot()
{
    score += 10;
    // Collecting carrots increases the score
    carrotCount++;
    UpdateUI();
}

public void RegisterMissedHop()
{
    
    score = Mathf.Max(0, score - 5);
    // Missing beats or off-rhythm inputs deduct points from the player's total
    UpdateUI();
}
```

 I wanted obstacles to spawn on each beat to give players a visual cue for when to press the spacebar. To achieve this, I created a script that references the Procedural Audio Generator and triggers obstacle spawning in sync with the beat.
 
 ```
 private void OnEnable()
{
    ProceduralAudioSequencer.OnBeatTick += SpawnObstacleOnBeat;

    // References the Procedural Audio Generator and listens for beat tick events
}

private void SpawnObstacleOnBeat()
{
    Instantiate(obstaclePrefab, spawnPosition.position, Quaternion.identity);

     // Spawns obstacles on each beat tick to give players a visual cue 
}
```
 
  I also implemented a world scroller that moves the environment to the left, creating the illusion that the bunny is progressing through the level. 

  ```
  private void Update()
{
    transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);
    // Implements a world scroller that continuously moves environment tiles and obstacles left

    if (transform.position.x <= resetThreshold)
    {
        RepositionTile();
        // Creates the illusion that the bunny is actively progressing forward through the level
    }
}
```

## Final outcome (173)
The final outcome was a rhythm game that uses procedural audio generation to influence gameplay. The player controls a bunny that hops in time with the music while avoiding obstacles and collecting carrots. Every 16 beats, the system randomly selects from the stored notes to create variation in the music and reduce repetition. The obstacles spawn in sync with the beat, connecting the audio to the gameplay. Once the player collects the required number of carrots, the next level begins. The game can be played through a web browser on Itch.io. One challenge I faced was making sure that the obstacles spawned in time with the procedurally generated music. As the audio was being generated through code, I needed the obstacles to follow the same beat timing rather than using a separate timer. To overcome this, I created a script that references the Procedural Audio Generator and triggers obstacle spawning whenever a beat plays. This helped keep the gameplay synchronised with the music and taught me the importance of connecting different systems together.

[YouTube video of Bunny Hop](https://youtu.be/KtQEUB-oMFw)

[Itch build](https://starcreations.itch.io/bunny-hop)
## Skills gained (71)
I gained an understanding of procedural audio generation and how music can be created through code rather than using prerecorded assets. I also learnt how to synchronise gameplay events with audio timing. This is valuable because the code is reusable across other projects, reduces my reliance on external assets and can make development more time-efficient. This experience has helped me understand how procedural generation can create variation and influence player interaction. 



## Sources 

Making Procedural Music in Unity (2022) Directed by Ryan Hedgecock. At: https://www.youtube.com/watch?v=FQnDgMuqqEw (Accessed  28/10/2026).

MCV Editors (2012) 'Procedural audio with Unity' In: MCV 20/07/2012 At: https://mcvuk.com/development-news/procedural-audio-with-unity/ (Accessed  28/10/2026).

robrt60 (2021) An Ultimate Guide to Musical Frequencies | Understand Them like a Pro. At: https://www.idrumtune.com/ultimate-guide-to-musical-frequencies/ (Accessed  28/10/2026).

Geometry Dash - Official Website (s.d.) At: https://www.geometrydash.com/ (Accessed  28/10/2026).

## Declaration 

Gemini was used to help teach me the code and implement it inside my game. 