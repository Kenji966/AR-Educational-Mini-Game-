/// <summary>
/// Game manager for AR educational mini-game.
/// Handles game state, AR object spawning, touch interactions, and multilingual dialogue switching.
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour
{
   [Header("Number Pools")]
	[Tooltip("List of all candidate numbers (strings) used for random target selection.")]
    private static List<string> numberList = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
	
	[Tooltip("Numbers available for spawning as AR objects.")]
    private List<string> spawnNumberList = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };
	
    [Tooltip("Numbers to check for matching when user selects an AR object.")]
	private List<string> checkNumberList = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };


	[Header("State Tracking")]
    private int randomNumber;
	
	[Tooltip("Current randomly selected target number.")]
    private string currentPointer = "";
	
	[Tooltip("The last spawned number's string value.")]
	private string currentSpawnPointer = "";
	
	[Tooltip("Counter for spawned planes.")]
    private int planeCount = 0;
	
	[Tooltip("Counter for spawned AR objects.")]
	private int spawnCount = 0;


	[Header("Prefab & AR References")]
    private List<Transform> planePositions = new List<Transform>();
	
	 
	[Tooltip("Prefab pool for AR number objects.")]
    public GameObject[] numberPrefabs;
	
	[Tooltip("Reference to AR camera.")]
    private static Camera arCamera;
	
	[Tooltip("List of all positions of spawned objects.")]
     private List<Transform> spawnPositions = new List<Transform>();
	 
   
	[Tooltip("Handles AR raycasting for plane/object detection.")]
    private ARRaycastManager arRaycastManager;
	
	[Tooltip("Flag indicating if the spawn position is sufficiently far from all other spawned objects.")]
    private bool isDistanceOk = false;
	
	[Tooltip("Reference to the object effect script on the selected AR object.")]
    private objectEffect _objectEffect;

	[Header("Game State References")]
	[Tooltip("Current game status. 0: Not started, 1: Playing, 2: Next round, 3: Game ended.")]
    public static int gameStatus = 0;
	
	[Tooltip("Reference to the Start/Gameplay/End scene GameObject.")]
    public GameObject startScene, gameScene, endScene;
	
	[Tooltip("Effect played when the player selects the correct/wrong object.")]
	public GameObject trueEffect, wrongEffect; 
	
	[Tooltip("Dialogue UI GameObject for displaying messages.")]
	public GameObject dialogueObject; 
	
	
	[Tooltip("Score UI GameObject displayed after collecting the correct object.")]
	public GameObject getScoreFrame, getScoreboard;
	
	
    [Tooltip("Reference to the main dialogue script for text/voice interaction.")]
	public DialogueUIController dialogueScript;

	[Tooltip("Reference to the sub-dialogue script for feedback dialogues.")]
	public DialogueUIController subdialogueScript;

	[Tooltip("Reference to the win-dialogue script for victory messages.")]
	public DialogueUIController winDialogueScript;

	
	[Header("UI References")]
	[Tooltip("Reference to the main UI canvas RectTransform.")]
    public RectTransform canvasRectTransform;
	
	[Tooltip("Touch effect UI GameObject for feedback on player touch.")]
    public GameObject touchEffect;
    
	[Tooltip("Coroutine handler for object spawning. Used for controlling timing of AR number placement.")]
	private IEnumerator spawnCoroutine;
    
	[Tooltip("Delay in seconds between each AR number spawn attempt.")]	
	private float spawnDelay = 1.25f;


    [Header("Language & Audio")]
	[Tooltip("Current language index. 0: English, 1: Japanese.")]
	public static int languageIndex = 0;

	[Tooltip("Button to switch language.")]
    public GameObject engButton, jpButton;

	[Tooltip("Reference to the AudioSource for playing sound effects.")]
    private AudioSource _audio;
    
	[Tooltip("Audio clips for item collection (index 0: correct, index 1: wrong).")]
	public AudioClip[] getItemAudio;






////////////////////////////////////////////////////////////////////////////////////////////////////////////
///
/// Initializes AR Raycast Manager and core game state at launch.
/// Sets up random target number and prepares game scene based on status.
///
////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private void Awake()
    {
        arRaycastManager = GameObject.Find("AR Session Origin").GetComponent<ARRaycastManager>();

        _audio = GetComponent<AudioSource>();
        randomNumber = Random.Range(0, numberList.Count); 
        currentPointer = numberList[randomNumber];
        numberList.Remove(currentPointer);

    
        switch (gameStatus) 
        {
            case 0:
                startScene.SetActive(true);
                break;
            case 1:
                startScene.SetActive(false);
                gameScene.SetActive(true);
                break;
            case 2:
                startScene.SetActive(false);
                gameScene.SetActive(true);
                break;
            case 3:
                startScene.SetActive(false);
                gameScene.SetActive(false);
                endScene.SetActive(true);
                winDialogueScript.SetDialogueType(gameStatus, languageIndex);
                break;

        }

        dialogueScript.Index = gameStatus;
        dialogueScript.randomNumber = randomNumber;
    }
	
	
////////////////////////////////////////////////////////////////////////////////////////////////////////////
///
/// Switches UI and internal data to the selected language (English/Japanese).
/// Updates dialogue system accordingly.
///
////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public void LanguageChange() 
    {
        if (languageIndex == 0)
        {
            engButton.SetActive(false);
            jpButton.SetActive(true);
            languageIndex = 1;
        }
        else
        {
            engButton.SetActive(true);
            jpButton.SetActive(false);
            languageIndex = 0;
        }
        dialogueScript.SetDialogueType(gameStatus,languageIndex);
    }






////////////////////////////////////////////////////////////////////////////////////////////////////////////
///
/// Immediately starts the main game logic and enables gameplay UI.
///
////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void StartGame() 
    {
        startScene.SetActive(false);
        gameScene.SetActive(true);
        gameStatus = 1;
        dialogueScript.Index = gameStatus;
    }






////////////////////////////////////////////////////////////////////////////////////////////////////////////
///
/// Handles per-frame logic for object spawning and touch interactions.
/// Controls spawning logic and processes player touch input for AR object detection and feedback.
///
////////////////////////////////////////////////////////////////////////////////////////////////////////////
    void Update()
    {

        if (gameStatus==0 || gameStatus==3)
            return;
        

        if (planeCount < planePositions.Count)
        { 
            if(spawnCount<10)
            {
                spawnCoroutine = SpawnObjectCoroutine(Random.Range(1, 4), planePositions[planeCount]);
                StartCoroutine(spawnCoroutine);
            }
            planeCount += 1;
        }


        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            touchEffect.GetComponent<Animator>().Play("touchEffect"); 
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, touch.position, null, out localPoint);
            touchEffect.GetComponent<RectTransform>().anchoredPosition = localPoint;
           
            if (!dialogueScript.isDialogueFinished)
                return;
           
            if (touch.phase == TouchPhase.Began)
            {
                Ray ray = arCamera.ScreenPointToRay(touch.position);
                RaycastHit getObject;
                
                if (Physics.Raycast(ray, out getObject))
                {
                     _objectEffect = getObject.transform.GetComponent<objectEffect>();
                    if (_objectEffect != null ) 
                    {
                        if (_objectEffect.isGot)
                            return;
                        _objectEffect.beTarget();
                      
                        if (_objectEffect._index == currentPointer)
                        {
                            _audio.PlayOneShot(getItemAudio[0]);
                            getScoreFrame.SetActive(true);
                            getScoreboard.SetActive(true);
                            getScoreboard.GetComponentInChildren<DialogueUIController>().SetDialogueType(5, languageIndex);
                            Invoke("GetScore", 10.5f);
                        }
                        else
                        {
                            _audio.PlayOneShot(getItemAudio[1]);
                            subdialogueScript.SetDialogueType(6, languageIndex);
                            dialogueObject.GetComponent<Animator>().Play("DialogueAni2");
                        }
                    }
                }

            }
        }

    }


	
	
	
////////////////////////////////////////////////////////////////////////////////////////////////////////////
///
/// Handles scoring and scene transition after completing a game round.
///
////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public void GetScore()
    {
        print(numberList.Count);
        if (numberList.Count >=0)
            gameStatus = 2;
        else
            gameStatus = 3;
        SceneManager.LoadScene("GameScene");
    }





////////////////////////////////////////////////////////////////////////////////////////////////////////////
///
/// Spawns a random number of AR objects at the specified position,
/// ensuring no overlap and reasonable distance from the camera.
///
////////////////////////////////////////////////////////////////////////////////////////////////////////////
    IEnumerator SpawnObjectCoroutine(int i, Transform pos)
    {
        yield return new WaitForSeconds(spawnDelay);
        for (int i2 = 0; i2 < i; i2++)
        {
            float ranX = Random.Range(-0.1f, 0.1f);
            float ranZ = Random.Range(-0.1f, 0.1f);
            int randomNumberSpawn = Random.Range(0, spawnNumberList.Count);


            if (spawnPositions.Count > 0)
            {
                for (int checkDistanceI = 0; checkDistanceI < spawnPositions.Count; checkDistanceI++)
                {
                    if (spawnPositions[checkDistanceI] != null)
                    {
                        if (Vector3.Distance(new Vector3(pos.position.x + ranX, pos.position.y, pos.position.z + ranZ), spawnPositions[checkDistanceI].position) > .15f)
                            isDistanceOk = true;
                        else
                            isDistanceOk = false;
                    }
                }
            }


            if (Vector3.Distance(new Vector3(pos.position.x + ranX, pos.position.y, pos.position.z + ranZ), arCamera.transform.position) > 1.5f && isDistanceOk
                || spawnPositions.Count == 0 && Vector3.Distance(new Vector3(pos.position.x + ranX, pos.position.y, pos.position.z + ranZ), arCamera.transform.position) > .5f)
            {
                for (int SpawnI = 0; SpawnI < checkNumberList.Count; SpawnI++)
                {
                    if (checkNumberList[SpawnI] == spawnNumberList[randomNumberSpawn])
                    {
                        GameObject spawnedfb = Instantiate(numberPrefabs[SpawnI], new Vector3(pos.position.x + ranX, pos.position.y, pos.position.z + ranZ), numberPrefabs[randomNumberSpawn].transform.rotation);
                        spawnPositions.Add(spawnedfb.transform); currentSpawnPointer = spawnNumberList[randomNumberSpawn];
                        spawnNumberList.Remove(currentSpawnPointer);
                        spawnCount += 1;
                        break;
                    }
                    isDistanceOk = false;
                    yield return new WaitForSeconds(spawnDelay);
                }

            }
        }

    }


 }
