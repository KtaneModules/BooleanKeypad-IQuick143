using System.Collections;
using UnityEngine;

[RequireComponent(typeof(KMSelectable))]
public class Button : MonoBehaviour {
	[SerializeField]
	private Material OFF;
	[SerializeField]
	private Material Correct;
	[SerializeField]
	private Material Wrong;
	public KMSelectable button;
	[SerializeField]
	private new KMAudio audio;
	[SerializeField]
	private MeshRenderer LED;
	[SerializeField]
	private TextMesh text;

	// Use this for initialization
	void Awake() {
		this.button = this.GetComponent<KMSelectable>();
		this.button.OnInteract += OnInteract;
	}

	public void FlashRED(float duration = 0.1f) {
		StartCoroutine(LEDRedRoutine(duration));
	}

	public void TurnON() {
		this.LED.material = this.Correct;
	}

	private IEnumerator LEDRedRoutine(float duration = 1.0f) {
		this.LED.material = this.Wrong;
		yield return new WaitForSeconds(duration);
		this.LED.material = this.OFF;
	}

	public void SetLabel(string label) {
		this.text.text = label;
	}

	private bool OnInteract() {
		this.button.AddInteractionPunch();
		this.audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, this.transform);
		return false;
	}
}
