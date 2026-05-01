using System;
using Ink.Runtime;
using Spine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// This is a super bare bones example of how to play and display a ink story in Unity.
public class BasicInkExample : MonoBehaviour {
	public static event Action<Story> OnCreateStory;

	void Awake() {
		// Remove the default message
		RemoveChildren();
		StartStory();
	}

	// Creates a new Story object with the compiled story which we can then play!
	void StartStory() {
		story = new Story(inkJSONAsset.text);
		if (OnCreateStory != null) OnCreateStory(story);
		RefreshView();
	}

	public void ContinueStory() {
		RefreshView();
	}

	// This is the main function called every time the story changes. It does a few things:
	// Destroys all the old content and choices.
	// Continues over all the lines of text, then displays all the choices. If there are no choices, the story is finished!
	void RefreshView() {
		// Remove all the UI on screen
		RemoveChildren();

		// Read all the content until we can't continue any more
		if (story.canContinue) {
			// Continue gets the next line of the story
			string text = story.Continue();
			// This removes any white space from the text.
			text = text.Trim();
			// Display the text on screen!
			SetContentText(text);
		}
		else if (story.currentChoices.Count > 0) {
			// Display all the choices, if there are any!
			for (int i = 0; i < story.currentChoices.Count; i++) {
				Choice choice = story.currentChoices[i];
				Button button = CreateChoiceView(choice.text.Trim());
				// Tell the button what to do when we press it
				button.onClick.AddListener(delegate {
					OnClickChoiceButton(choice);
				});
			}
		}
		else {
			Button choice = CreateChoiceView("End of story.\nRestart?");
			choice.onClick.AddListener(delegate {
				EndStroy();
			});
		}
		// If we've read all the content and there's no choices, the story is finished!
		// else {
		// }
	}

	void EndStroy() {
		StartStory();
	}

	// When we click the choice button, tell the story to choose that choice!
	void OnClickChoiceButton(Choice choice) {
		story.ChooseChoiceIndex(choice.index);
		RefreshView();
	}

	// Creates a textbox showing the the line of text
	void SetContentText(string text) {
		// Text storyText = Instantiate (textPrefab) as Text;
		contentText.text = text;
		// storyText.transform.SetParent (canvas.transform, false);
	}

	// Creates a button showing the choice text
	Button CreateChoiceView(string text) {
		// Creates the button from a prefab
		Button choice = Instantiate(buttonPrefab) as Button;
		choice.transform.SetParent(choiceArea.transform, false);
		choice.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 80);

		// Gets the text from the button prefab
		Text choiceText = choice.GetComponentInChildren<Text>();
		choiceText.text = text;

		// Make the button expand to fit the text
		// HorizontalLayoutGroup layoutGroup = choice.GetComponent<HorizontalLayoutGroup>();
		// layoutGroup.childForceExpandHeight = false;

		return choice;
	}

	// Destroys all the children of this gameobject (all the UI)
	void RemoveChildren() {
		int childCount = choiceArea.transform.childCount;
		for (int i = childCount - 1; i >= 0; --i) {
			Destroy(choiceArea.transform.GetChild(i).gameObject);
		}
	}

	[SerializeField]
	private TextAsset inkJSONAsset = null;
	public Story story;

	// [SerializeField]
	// private Canvas canvas = null;

	// UI Prefabs
	[SerializeField]
	private Text contentText = null;
	[SerializeField]
	private Button buttonPrefab = null;

	[SerializeField]
	private GameObject choiceArea = null;
}
