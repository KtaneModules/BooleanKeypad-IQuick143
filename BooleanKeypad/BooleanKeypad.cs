using UnityEngine;
using System.Text.RegularExpressions;

public class BooleanKeypad : MonoBehaviour {
	[SerializeField]
	private LED[] LEDs;
	[SerializeField]
	private Button[] Buttons;
	[SerializeField]
	private KMBombModule module;
	private static string[][] magicLetters = {
		new string[]{"ʯ", "Ӈ", "⌉", "Ҍ", "Ԭ", "ۺ"},
		new string[]{"ʭ", "Ƹ", "Ԋ", "ی", "Պ", "⊃"},
		new string[]{"Λ", "Φ", "Ȝ", "Ԓ", "֍", "Ͻ"},
		new string[]{"Ϯ", "Ь", "Ɋ", "Ҽ", "ד", "֎"},
		new string[]{"ω", "Ξ", "Ǯ", "Ӽ", "۞", "⌊"},
		new string[]{"Ъ", "Ϛ", "ۍ", "Ӷ", "ٻ", "Խ"}
	};

	private bool[] buttonTruths = new bool[4];
	private bool[] pressedButtons = new bool[]{false, false, false, false};

	private bool moduleSolved = false;

	// Things used for logging
	private static int moduleIdCounter = 1;
	private int moduleId;

	// Use this for initialization
	void Awake() {
		moduleId = moduleIdCounter;
		moduleIdCounter++;

		//Init shit idk
		for (int i = 0; i < 4; i++) {
			int j = i;
			Buttons[i].button.OnInteract += delegate() {HandlePress(j); return false;};
		}
	}

	void Start() {
		//Generate level
		for (int i = 0; i < 3; i++) {
			buttonTruths[Random.Range(0, 4)] = true;
		}
		bool[] ledStates = new bool[4];
		for (int i = 0; i < 4; i++) {
			ledStates[i] = Random.Range(0, 2) == 1;
			this.LEDs[i].SetState(ledStates[i]);
		}

		// Logging LEDs
		Debug.LogFormat("[Boolean Keypad #{0}] The top LEDs are {1} then {2}", moduleId, ledStates[0], ledStates[1]);
		Debug.LogFormat("[Boolean Keypad #{0}] The right LEDs are {1} then {2}", moduleId, ledStates[2], ledStates[3]);

		Operations[] buttonOperations = new Operations[4];

		// Logging Solution
		Debug.LogFormat("[Boolean Keypad #{0}] Press the correct buttons in reading order", moduleId);
		for (int i = 0; i < 4; i++) {
			bool[] OPvalues = CalculateOperations(ledStates[i % 2], ledStates[2 + i / 2]);
			int choice = Random.Range(0, 3);
			buttonOperations[i] = (Operations) choice + ((OPvalues[choice] == buttonTruths[i])?0:3);

			string label = magicLetters[(int) buttonOperations[i]][Random.Range(0, 6)];
			this.Buttons[i].SetLabel(label);
			
			// Logging buttons
			string op = buttonOperations[i].ToString();
			string answer = buttonTruths[i]?"Press this button":"Don't press this button";
		    Debug.LogFormat("[Boolean Keypad #{0}] Button {1} has label {2} and operation {3}. Inputs are {4} and {5}. " + answer, moduleId, i+1, label, op,
				ledStates[i % 2], ledStates[2 + i / 2]);
		}
	}

	private void HandlePress(int id) {
		if (this.buttonTruths[id]) {
			this.Buttons[id].TurnON();
			if (this.moduleSolved) return;
			this.pressedButtons[id] = true;

			for (int i = 0; i < 4; i++) {
				if (this.buttonTruths[i] && !this.pressedButtons[i]) {
					if (i < id) {
						Debug.LogFormat("[Boolean Keypad #{0}] Button {1} pressed. This is not the next true button in reading order. Button {2} expected instead.", moduleId, id+1, i+1);
						Strike();
					} else {
						Debug.LogFormat("[Boolean Keypad #{0}] Button {1} correctly pressed. Next button is {2}.", moduleId, id+1, i+1);
					}
					return;
				}
			}

			Debug.LogFormat("[Boolean Keypad #{0}] Last button {1} correctly pressed. Module is now solved.", moduleId, id+1);
			this.moduleSolved = true;
			this.module.HandlePass();
		} else {
			Debug.LogFormat("[Boolean Keypad #{0}] Button {1} pressed. This is button is false. It should not be pressed at all.", moduleId, id+1);
			Strike();
		}
	}

	private void Strike() {
		if (this.moduleSolved) return;
		Debug.LogFormat("[Boolean Keypad #{0}] A strike was received. All previous inputs are cleared.", moduleId);
		for (int i = 0; i < 4; i++) {
			this.Buttons[i].FlashRED();
		}
		this.module.HandleStrike();
		this.pressedButtons = new bool[]{false, false, false, false};
	}

	[HideInInspector]
	public string TwitchHelpMessage = "To input a solution to the module. Use \"solve [button indices]\". Buttons are indexed 1-4 in reading order. Examples: solve 1 2 4 or solve 124";
	public KMSelectable[] ProcessTwitchCommand(string command) {
		command = command.ToLowerInvariant();
		var match = Regex.Match(command, "solve(?: [^1-4\n]*)?([1-4])[^1-4\n]*([1-4\n])?[^1-4\n]*([1-4])?[^1-4\n]*([1-4])?.*");
		if (!match.Success) return null;
		bool[] keys = new bool[4];
		int nKeys = 0;
		for (int i = 1; i < match.Groups.Count; i++) {
			int key = -1;
			if (int.TryParse(match.Groups[i].Value, out key) && key >= 1 && key <= 4) {
				if (keys[key-1] == false) {
					nKeys++;
				}
				keys[key-1] = true;
			}
		}
		if (nKeys == 0) return null;
		KMSelectable[] output = new KMSelectable[nKeys];
		for (int i = 0, j = 0; i < 4; i++) {
			if (keys[i]) {
				output[j] = this.Buttons[i].button;
				j++;
			}
		}
		return output;
	}

	//AND, OR, XOR
	private bool[] CalculateOperations(bool A, bool B) {
		return new bool[] {A && B, A || B, A != B};
	}

	private enum Operations : int {
		AND = 0,
		OR = 1,
		XOR = 2,
		NAND = 3,
		NOR = 4,
		XNOR = 5
	}
}
