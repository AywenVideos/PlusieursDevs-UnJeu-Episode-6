using UnityEngine;

[CreateAssetMenu(fileName = "SO_CharacterBody", menuName = "Scriptable Objects/SO_CharacterBody")]
public class SO_CharacterBody : ScriptableObject
{
    public BodyPart[] characterBodyParts;
}

[System.Serializable]
public class BodyPart
{
    public string bodyPartName;
    public SO_BodyPart bodyPart;
}
