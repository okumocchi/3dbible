using UnityEngine;

public class Marker
{
    public Vector3 position;
    public Vector3 localEulerAngles;

    public Marker(Transform t) {
        position = t.position;
        localEulerAngles = t.localEulerAngles;
    }

    public Marker(Marker m) {
        position = m.position;
        localEulerAngles = m.localEulerAngles;
    }

    public Marker(Vector3 _position, Vector3 _localEulerAngles) {
        position = _position;
        localEulerAngles = _localEulerAngles;
    }

}
