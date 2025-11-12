using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro 드롭다운을 사용하기 위해 필요
using UnityEngine.Localization.Settings; // Localization 시스템에 접근하기 위해 필요
using UnityEngine.Localization; // Locale 클래스를 사용하기 위해 필요

public class LanguageDropdown : MonoBehaviour
{
    // 1. 인스펙터에서 연결할 TMP_Dropdown
    public TMP_Dropdown dropdown;

    void Start()
    {
        // Localization 시스템이 준비될 때까지 기다리는 코루틴을 실행합니다.
        StartCoroutine(SetupDropdown());
    }

    IEnumerator SetupDropdown()
    {
        // 2. Localization 시스템이 초기화(로드)될 때까지 대기합니다.
        // (이걸 하지 않으면 사용 가능한 언어 목록을 가져올 수 없습니다.)
        yield return LocalizationSettings.InitializationOperation;

        // 3. 드롭다운의 기존 옵션을 모두 삭제합니다.
        dropdown.ClearOptions();

        // 4. 사용 가능한 모든 언어(Locale) 목록을 가져옵니다.
        var locales = LocalizationSettings.AvailableLocales.Locales;
        List<string> options = new List<string>();
        int currentLocaleIndex = 0;

        // 5. 각 언어를 드롭다운 옵션 목록(List<string>)에 추가합니다.
        for (int i = 0; i < locales.Count; i++)
        {
            Locale locale = locales[i];

            // "한국어", "English", "日本語" 처럼 해당 언어의 고유 이름을 가져옵니다.
            string displayName = locale.Identifier.CultureInfo.NativeName;

            // (선택 사항) "Korean", "English (United States)" 처럼 영어 기반 이름을 원한다면
            // string displayName = locale.LocaleName;

            options.Add(displayName);

            // 6. 현재 선택된 언어가 목록의 몇 번째인지 확인합니다.
            if (locale == LocalizationSettings.SelectedLocale)
            {
                currentLocaleIndex = i;
            }
        }

        // 7. 준비된 옵션 목록(options)을 드롭다운에 적용합니다.
        dropdown.AddOptions(options);

        // 8. 드롭다운의 기본값을 현재 언어로 설정합니다.
        // (리스너를 등록하기 전에 value를 설정해야 불필요한 이벤트 호출을 막을 수 있습니다.)
        dropdown.value = currentLocaleIndex;
        dropdown.RefreshShownValue(); // 화면에 현재 값을 즉시 표시

        // 9. 드롭다운의 값이 변경될 때마다 OnLanguageChanged 함수가 호출되도록 이벤트를 등록합니다.
        dropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    // 10. 드롭다운에서 새로운 언어를 선택했을 때 호출되는 함수
    private void OnLanguageChanged(int index)
    {
        // 선택된 인덱스(index)에 해당하는 언어로 시스템의 기본 언어를 변경합니다.
        // 이 한 줄만으로도 LocalizationEvent가 연결된 모든 텍스트와 에셋이 자동 변경됩니다.
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
    }
}