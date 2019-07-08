using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class LED : MonoBehaviour {
	[SerializeField]
	private new Light light;
	[SerializeField]
	private Material ON;
	[SerializeField]
	private Material OFF;
	private new MeshRenderer renderer;

	private void Awake() {
		this.renderer = this.GetComponent<MeshRenderer>();
		//Fix for different size bomb lights
		this.light.range *= this.light.transform.lossyScale.x;
	}

	public void SetState(bool state) {
		this.renderer.material = state?this.ON:this.OFF;
		this.light.enabled = state;
	}
}
