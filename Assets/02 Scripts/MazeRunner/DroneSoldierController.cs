using UnityEngine;

public sealed class DroneSoldierController : MonoBehaviour
{
    public Transform[] rotors;

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        foreach (var rotor in this.rotors)
            rotor.RotateAround(rotor.GetObjectCenter(), this.transform.up, 3600 * Time.deltaTime);
    }
}