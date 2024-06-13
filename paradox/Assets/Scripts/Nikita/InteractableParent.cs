using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableParent : MonoBehaviour
{
    #region objName
    [SerializeField]
    protected string objName = "null";
    public string ObjName {
        get { return objName; }
    }
    #endregion

    public bool isInteractable = true;

    #region objTags
    [SerializeField]
    protected List<string> objTags;
    public List<string> ObjTags {
        get {
            return objTags;
        }
    }
    public bool GotObjTag(string tag) {
        foreach (string item in objTags) {
            if (item == tag)
                return true;
        }
        return false;
    }
    public bool AddObjTag(string tag) {
        if (GotObjTag(tag)) {
            return false;
        }
        objTags.Add(tag);
        return true;
    }
    public bool RemoveObjTag(string tag) {
        for (int i = 0; i < objTags.Count; i++) {
            if (objTags[i] == tag) {
                objTags.RemoveAt(i);
                return true;
            }
        }
        return false;
    }
    #endregion

    #region lineTags
    protected string[] GetLineTags(string tags) {
        return tags.Split('_');
    }
    protected bool GotLineTag(string[] tags, string tag) {
        foreach (string item in tags) {
            if (tag == item)
                return true;
        }
        return false;
    }
    #endregion

    virtual public void OnInteract() {
        
    }
}