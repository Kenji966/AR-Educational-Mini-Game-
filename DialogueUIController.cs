/// <summary>
/// Controls UI typewriter effect and voice playback for multilingual dialogue.
/// Supports dynamic dialogue and audio switching between English and Japanese,
/// and is designed for AR educational game UI.
/// </summary>

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUIController : MonoBehaviour
{ 
    [Header("Get Main GameManager")]
	public Manager gameManager;
	
	
    [Header("Dialogue Controll")]
	public int numberIndex, languageIndex;
	public int dialogueType,  randomNumber;
	
	[SerializeField] 
	private TMP_Text dialogueText;
	
	
	
////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// Dialogue string arrays for each supported language.
/// English: dialogueListA, dialogueListB, TrueDialogue, FalseDialogue
/// Japanese: jp_dialogueListA, jp_dialogueListB, jp_TrueDialogue, jp_FalseDialogue
////////////////////////////////////////////////////////////////////////////////////////////////////////////
	[Header("Dialogue Content List")]
	public string[] currentDialogue, dialogueListA, dialogueListB, TrueDialogue, FalseDialogue, 
		winDialogue= { "You are the best! Congratulations on completing the game.", "良くやったね、ゲームをクリアおめでとうございます。" },
		jp_dialogueListA = { "こんにちは〜ゲームを始める前に、ゲームの遊び方を理解しましょう。", "私が数字を言ったら、その音に基づいて数字を見つけましょう。", "さあ、始めましょう。", "今、私たちは数字を探しています。一緒に探しながら、私についてと言ってみましょう。" },
		jp_dialogueListB = { "とても良いですね、次に挑戦しましょう。", "さあ、始めましょう。" },
		jp_TrueDialogue = { "素晴らしい！", "頑張って！", "ブラボー" },
		jp_FalseDialogue = { "頑張って！できるよ！", "もう少しで目標に届きますよ！" };
		
	public int dialogueInt = 0;
	[SerializeField] private Coroutine currentCoroutine;


    [Header("Typing Effect Controll")]
	 float delayBeforeStart = 1.5f, delayBeforeNext = 0.5f;
	[SerializeField] private float timeBtwChars = 0.00001f;
	[SerializeField] private string leadingChar = "";
	[SerializeField] private bool leadingCharBeforeDelay = false;
	[Space(10)] [SerializeField] private bool startOnEnable = false;

	

    [Header("Voice Audio Content")]
	public AudioClip[] dialogueAudioNow, NumberAudio, jp_NumberAudio, dialogueAudioListA, dialogueAudioListB, jp_dialogueAudioListA, jp_dialogueAudioListB, TrueAudioList, FalseList, JP_TrueAudioList, JP_FalseAudioList;
	
    [Header("Voice Audio Controll")]
	public AudioSource _audio;
	public AudioClip[] winAudio;
	public int currentDialogueIndex = 0;
	public bool isDialogueFinished = false;



////////////////////////////////////////////////////////////////////////////////////////////////////////////
///
/// MonoBehaviour Start: Initializes audio source, sets up dialogue based on dialogueType and language, and starts the corresponding dialogue coroutine.
///
////////////////////////////////////////////////////////////////////////////////////////////////////////////
	void Start()
	{
		if(_audio==null)
			_audio = GameObject.Find("GameScene").GetComponent<AudioSource>();
		dialogueText.text = "";

		

		switch (dialogueType) 
		{
			case 1:
				if (languageIndex == 0)
				{
					currentDialogue = dialogueListA;
					dialogueAudioNow = dialogueAudioListA;
				}
				else 
				{
					currentDialogue = jp_dialogueListA;
					dialogueAudioNow = jp_dialogueAudioListA;
				}				
				numberIndex = gameManager.randomNumber;
				StartCoroutine("MaindialogueWriter");
				break;
			case 2:
				if (languageIndex == 0)
				{
					currentDialogue = dialogueListB;
					dialogueAudioNow = dialogueAudioListB;
				}
				else
				{
					currentDialogue = jp_dialogueListB;
					dialogueAudioNow = jp_dialogueAudioListB;
				}
				numberIndex = gameManager.randomNumber;
				StartCoroutine("MaindialogueWriter");
				break;
			case 3:
				StartCoroutine("dialogueWriter");
				break;
			
		}

	}
	
	

////////////////////////////////////////////////////////////////////////////////////////////////////////////
///
/// Sets the dialogue type and language for the UI.
/// Selects appropriate dialogue and audio array based on type and language.
/// </summary>
/// <param name="dialogueType">1: Main, 2: Sub, 3: True, 4: False</param>
/// <param name="languageIndex">0: English, 1: Japanese</param>
///
////////////////////////////////////////////////////////////////////////////////////////////////////////////
	public void SetDialogueType(int _dialogueType, int _languageIndex) 
	{
		dialogueType = _dialogueType;
		languageIndex = _languageIndex;

		dialogueText.text = "";
		switch (dialogueType)
		{
			case 1:
				if (languageIndex == 0)
				{
					currentDialogue = dialogueListA;
					dialogueAudioNow = dialogueAudioListA;
				}
				else
				{
					currentDialogue = jp_dialogueListA;
					dialogueAudioNow = jp_dialogueAudioListA;
				}
				numberIndex = gameManager.randomNumber;
				StartCoroutine("MaindialogueWriter");
				break;
			case 2:
				if (languageIndex == 0)
				{
					currentDialogue = dialogueListB;
					dialogueAudioNow = dialogueAudioListB;
				}
				else
				{
					currentDialogue = jp_dialogueListB;
					dialogueAudioNow = jp_dialogueAudioListB;
				}
				numberIndex = gameManager.randomNumber;
				StartCoroutine("MaindialogueWriter");
				break;
			
			case 3:
				StartCoroutine("SubdialogueWriter");
				if (languageIndex == 0)
				{
					dialogueAudioNow = TrueAudioList;
					currentDialogue = TrueDialogue;
                }
                else {
						dialogueAudioNow = JP_TrueAudioList;
						currentDialogue = jp_TrueDialogue;
					}
				break;
			case 4:
				StartCoroutine("SubdialogueWriter");
				if (languageIndex == 0)
				{
					dialogueAudioNow = FalseList;
					currentDialogue = FalseDialogue;
				}
				else
				{
					dialogueAudioNow = JP_FalseAudioList;
					currentDialogue = jp_FalseDialogue;
				}
				break;
		}
	}



////////////////////////////////////////////////////////////////////////////////////////////////////////////
///
/// Coroutine to display main dialogue with Typewriter effect.
/// Plays the corresponding Voice for each dialogue line.
/// On the last line, plays the random number audio (multilingual).
///
////////////////////////////////////////////////////////////////////////////////////////////////////////////
    IEnumerator MaindialogueWriter()
	{
		dialogueText.text = leadingCharBeforeDelay ? leadingChar : "";
		

		yield return new WaitForSeconds(delayBeforeStart);

		for (int j = 0; j < currentDialogue.Length; j++)
		{
			dialogueText.text = "";
			if (j == currentDialogue.Length - 1)
			{
				isDialogueFinished = true;
				if (languageIndex == 0)
					_audio.PlayOneShot(NumberAudio[numberIndex]);
				else
					_audio.PlayOneShot(jp_NumberAudio[numberIndex]);

				foreach (char c in currentDialogue[currentDialogue.Length - 1])
				{
					if (dialogueText.text.Length > 0)
					{
						dialogueText.text = dialogueText.text.Substring(0, dialogueText.text.Length - leadingChar.Length);
					}
					dialogueText.text += c;
					dialogueText.text += leadingChar;
					
					yield return new WaitForSeconds(timeBtwChars);
				}
				j -= 1;
				yield return new WaitForSeconds(UnityEngine.Random.Range(6,12)); //UnityEngine.Random.Range(3,5)
			}
			else
			{
				_audio.PlayOneShot(dialogueAudioNow[j]);
				foreach (char c in currentDialogue[j])
				{
					if (dialogueText.text.Length > 0)
					{
						dialogueText.text = dialogueText.text.Substring(0, dialogueText.text.Length - leadingChar.Length);
					}
					dialogueText.text += c;
					dialogueText.text += leadingChar;
					yield return new WaitForSeconds(timeBtwChars);
				}
			}
			currentDialogueIndex = j; 
			if (languageIndex == 1)
				delayBeforeStart = 10;
			yield return new WaitForSeconds(delayBeforeNext);
		}
		if (leadingChar != "")
		{
			dialogueText.text = dialogueText.text.Substring(0, dialogueText.text.Length - leadingChar.Length);
		}
	}


	
////////////////////////////////////////////////////////////////////////////////////////////////////////////
///
/// Coroutine to display a random feedback dialogue (True/False) with typewriter effect.
/// Selects a random line from the appropriate language and type.
///
////////////////////////////////////////////////////////////////////////////////////////////////////////////
	IEnumerator SubdialogueWriter()
	{
		dialogueText.text = leadingCharBeforeDelay ? leadingChar : "";
		

		int ran_dialogue = UnityEngine.Random.Range(0, currentDialogue.Length);
		yield return new WaitForSeconds(delayBeforeStart);

			dialogueText.text = "";
			_audio.PlayOneShot(dialogueAudioNow[ran_dialogue]);
			print("subDialogue :" + currentDialogue[ran_dialogue]);
			foreach (char c in currentDialogue[ran_dialogue])
			{
				if (dialogueText.text.Length > 0)
				{
					dialogueText.text = dialogueText.text.Substring(0, dialogueText.text.Length - leadingChar.Length);
				}
				dialogueText.text += c;
				dialogueText.text += leadingChar; 
				
				yield return new WaitForSeconds(timeBtwChars);
			}

		if (leadingChar != "")
		{
			dialogueText.text = dialogueText.text.Substring(0, dialogueText.text.Length - leadingChar.Length);
		}
	}
	
	
	
////////////////////////////////////////////////////////////////////////////////////////////////////////////
///
/// Coroutine to display the win message in selected language with typewriter effect.
///
////////////////////////////////////////////////////////////////////////////////////////////////////////////
	IEnumerator dialogueWriter( )
	{
		dialogueText.text = leadingCharBeforeDelay ? leadingChar : "";
		
		
		yield return new WaitForSeconds(delayBeforeStart);
		dialogueText.text = "";
		_audio.PlayOneShot(winAudio[languageIndex]);
		foreach (char c in winDialogue[languageIndex])
		{
			if (dialogueText.text.Length > 0)
			{
				dialogueText.text = dialogueText.text.Substring(0, dialogueText.text.Length - leadingChar.Length);
			}
			dialogueText.text += c;
			dialogueText.text += leadingChar;
			yield return new WaitForSeconds(timeBtwChars);
		}

		if (leadingChar != "")
		{
			dialogueText.text = dialogueText.text.Substring(0, dialogueText.text.Length - leadingChar.Length);
		}
	}
}