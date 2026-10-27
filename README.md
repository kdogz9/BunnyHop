# Bunny Hop Dev log 


## Planning (73)
Firstly, I thought about procedural generation and what I wanted to gain from it. I decided to explore procedural audio generation and turn it into a rhythm game. I wanted the music to be the main feature rather than something that usually plays in the background of games. I watched YouTube tutorials to help guide me and used games such as Geometry Dash as inspiration to understand how rhythm-based gameplay could engage players.

## Development (232)
I started by following a YouTube tutorial that explained how procedural audio generation works. Using this as a guide, I created a script called Procedural Audio Generator. This converts BPM into time intervals between beats and uses Random.Range to randomly select notes from a musical scale. It then generates waveform samples directly in RAM and plays them. The system continues generating notes, creating a procedural audio sequence. I chose to generate the audio samples in RAM rather than relying on prerecorded audio files, allowing the audio to be created at runtime for WebGL. I then created a script to manage player input and game logic. This controls the bunny's hopping when the player presses the spacebar. It calculates the time difference between the player's input and the latest beat. If the input falls within the allowed timing window, it registers as a successful hop. The script also manages the scoring system. Collecting carrots increases the score, while missing beats or colliding with obstacles deducts points. I wanted obstacles to spawn on each beat to give players a visual cue for when to press the spacebar. To achieve this, I created a script that references the Procedural Audio Generator and triggers obstacle spawning in sync with the beat. I also implemented a world scroller that moves the environment to the left, creating the illusion that the bunny is progressing through the level. 

## Final outcome (173)
The final outcome was a rhythm game that uses procedural audio generation to influence gameplay. The player controls a bunny that hops in time with the music while avoiding obstacles and collecting carrots. Every 16 beats, the system randomly selects from the stored notes to create variation in the music and reduce repetition. The obstacles spawn in sync with the beat, connecting the audio to the gameplay. Once the player collects the required number of carrots, the next level begins. The game can be played through a web browser on Itch.io. One challenge I faced was making sure that the obstacles spawned in time with the procedurally generated music. As the audio was being generated through code, I needed the obstacles to follow the same beat timing rather than using a separate timer. To overcome this, I created a script that references the Procedural Audio Generator and triggers obstacle spawning whenever a beat plays. This helped keep the gameplay synchronised with the music and taught me the importance of connecting different systems together.

{Insert gameplay video}

## Skills gained (71)
I gained an understanding of procedural audio generation and how music can be created through code rather than using prerecorded assets. I also learnt how to synchronise gameplay events with audio timing. This is valuable because the code is reusable across other projects, reduces my reliance on external assets and can make development more time-efficient. This experience has helped me understand how procedural generation can create variation and influence player interaction. 

(549 words in total)

## Sources 
{Insert YouTube tutorial, Geometry Dash reference and any documentation used.}