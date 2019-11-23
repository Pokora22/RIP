using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


public abstract class Artifact : ScriptableObject
{
    public string m_name, m_description;
    public Sprite m_spriteActive, m_spriteInactive;

    public abstract bool activate();
    public abstract bool deactivate();

    public override string ToString()
    {
        return m_name +": " + m_description;
    }
}
