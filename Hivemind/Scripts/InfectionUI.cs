using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class InfectionUI : MonoBehaviour {

    public GameObject UIPanelPrefab;
    
    void Start()
    {
        CharacterManager.OnInfectionAdvance += CharacterManager_OnInfectionAdvance;
        CharacterManager.OnNewInfectedCharacter += CharacterManager_OnNewInfectedCharacter;
        CharacterManager.OnCharacterDeath += CharacterManager_OnCharacterDeath;
    }

    void CharacterManager_OnCharacterDeath(int i)
    {
        transform.GetChild(i).GetComponent<Image>().fillAmount = 1;
        Destroy(transform.GetChild(i).gameObject);
    }

    void CharacterManager_OnNewInfectedCharacter(EntityData ed)
    {
        SpawnInfectionDisplayer(ed);
    }

    void SpawnInfectionDisplayer(EntityData ed)
    {
        GameObject go = (GameObject)Instantiate(UIPanelPrefab, transform, false);
        go.transform.FindChild("Character").GetComponent<Text>().text = ed.character.characterName;
        go.transform.FindChild("InfectionStage").GetComponent<Text>().text = ed.currentStateOfInfection.ToString();
        //go.GetComponent<Image>().sprite = e.character.characterDialogSprite; //faceSprite?
        go.GetComponent<Image>().fillAmount = (((float)ed.character.infectionStageDuration - (float)ed.currentInfectionStageDuration) / (float)ed.character.infectionStageDuration);
    }

    void CharacterManager_OnInfectionAdvance()
    {
        for (int i = 0; i < CharacterManager.instance.infectedCharacters.Count; i++)
        {
            if (transform.childCount < CharacterManager.instance.infectedCharacters.Count)
            {
                SpawnInfectionDisplayer(CharacterManager.instance.infectedCharacters[i]);
            }

            EntityData ed = CharacterManager.instance.infectedCharacters[i];
            transform.GetChild(i).FindChild("InfectionStage").GetComponent<Text>().text = ed.currentStateOfInfection.ToString();
            transform.GetChild(i).GetComponent<Image>().fillAmount = (((float)ed.character.infectionStageDuration - (float)ed.currentInfectionStageDuration) / (float)ed.character.infectionStageDuration);
        }
    }
}
