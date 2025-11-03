using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LocalSelectorManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown localDropdown;
 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(LoadLocales());
    }
    IEnumerator LoadLocales(){
        yield return LocalizationSettings.InitializationOperation;
        localDropdown.ClearOptions();
 
        List<string> options = new List<string>();
        int currentIndex = 0;
        var locales = LocalizationSettings.AvailableLocales.Locales;
 
        for (int i = 0; i < locales.Count; i++){
            options.Add(locales[i].LocaleName);
 
            if (locales[i] == LocalizationSettings.SelectedLocale){
                currentIndex = i;
            }
        }
 
        localDropdown.AddOptions(options);
        localDropdown.value = currentIndex;
        localDropdown.onValueChanged.AddListener(OnLocalChanged);
    }
 
    void OnLocalChanged(int index){
        var selectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
        LocalizationSettings.SelectedLocale = selectedLocale;
    }
}
