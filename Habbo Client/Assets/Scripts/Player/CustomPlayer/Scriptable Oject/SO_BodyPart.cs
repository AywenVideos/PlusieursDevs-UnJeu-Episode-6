using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BodyPart", menuName = "Scriptable Objects/BodyPart")]
public class SO_BodyPart : ScriptableObject
{

    public string bodyPartName;
    public int bodyPartAnimationID;

    public List<AnimationClip> allBodyPartAnimations = new List<AnimationClip>();

}
