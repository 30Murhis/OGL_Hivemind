using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Characters : ScriptableObject
{
    [SerializeField]
    public List<Character> allCharacters;
}

/*
[System.Serializable]
public class Characters<T> : ScriptableObject
{
    //[SerializeField]
    public List<T> allCharacters;
}
*/

/*
[System.Serializable]
public class CharacterList : Characters<Character>
{
    [SerializeField]
    //public class Characters : Characters<Character> { }
    //public Characters characters;

    public Characters<Character> characters;
}
*/
