using UnityEngine;
using UnityEngine.Rendering;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    [SerializeField] private float dayDuration = 600f;
    [SerializeField] private float currentTime = 0f;

    [Header("Dynamic Lights")]
    [SerializeField] private Light sunLight;
    [SerializeField] private Light moonLight;
    [SerializeField] private float sunIntensityDay = 1.5f;
    [SerializeField] private float moonIntensityDay = 0f;
    [SerializeField] private float moonIntensityNight = 0.1f; // Уменьшено для более темной ночи

    [Header("Lighting Overrides")]
    [SerializeField] private Gradient sunColor;
    [SerializeField] private AnimationCurve sunIntensityCurve;
    [SerializeField] private AnimationCurve ambientIntensityMultiplier;

    [Header("Baked Lighting Support")]
    [SerializeField] private bool useBakedAmbient = true;
    [SerializeField] private float bakedLightingWeight = 0.7f;
    [SerializeField] private float dynamicLightingWeight = 0.3f;

    [Header("Night Darkness Settings")] // НОВЫЕ НАСТРОЙКИ
    [SerializeField] private float nightDarknessMultiplier = 0.3f; // Множитель темноты ночью (0-1)
    [SerializeField] private float minAmbientAtNight = 0.05f; // Минимальный ambient ночью
    [SerializeField] private AnimationCurve nightDarknessCurve; // Кривая темноты ночи
    [SerializeField] private bool enableDeepNight = true; // Включить глубокую ночь
    [SerializeField][Range(0f, 0.5f)] private float deepNightDuration = 0.3f; // Длительность глубокой ночи

    [Header("Performance")]
    [SerializeField] private float updateInterval = 0.1f;
    private float updateTimer = 0f;

    private Light additionalAmbientLight;

    void Start()
    {
        InitializeLights();
        UpdateLighting(0f);
    }

    void Update()
    {
        updateTimer += Time.deltaTime;
        if (updateTimer >= updateInterval)
        {
            UpdateTime();
            UpdateLighting(currentTime / dayDuration);
            updateTimer = 0f;
        }
    }

    void InitializeLights()
    {
        if (sunLight == null)
        {
            sunLight = GameObject.Find("Directional Light")?.GetComponent<Light>();
            if (sunLight == null)
            {
                GameObject sunGO = new GameObject("Sun Light");
                sunLight = sunGO.AddComponent<Light>();
                sunLight.type = LightType.Directional;
            }
        }

        sunLight.lightmapBakeType = LightmapBakeType.Mixed;
        sunLight.renderMode = LightRenderMode.Auto;

        if (moonLight == null)
        {
            GameObject moonGO = new GameObject("Moon Light");
            moonLight = moonGO.AddComponent<Light>();
            moonLight.type = LightType.Directional;
            moonLight.color = new Color(0.5f, 0.6f, 0.8f); // Более холодный и темный цвет луны
            moonLight.lightmapBakeType = LightmapBakeType.Mixed;
        }

        CreateAdditionalAmbientLight();
    }

    void CreateAdditionalAmbientLight()
    {
        GameObject ambientLightGO = new GameObject("Additional Ambient Light");
        additionalAmbientLight = ambientLightGO.AddComponent<Light>();
        additionalAmbientLight.type = LightType.Directional;
        additionalAmbientLight.lightmapBakeType = LightmapBakeType.Realtime;
        additionalAmbientLight.intensity = 0f;
        additionalAmbientLight.enabled = false;
    }

    void UpdateTime()
    {
        currentTime += updateInterval;
        if (currentTime >= dayDuration)
        {
            currentTime = 0f;
        }
    }

    void UpdateLighting(float timePercent)
    {
        UpdateSunAndMoon(timePercent);
        UpdateAmbientEnhancement(timePercent);
        UpdateNightDarkness(timePercent);
        UpdateLightProbes();
    }

    void UpdateSunAndMoon(float time)
    {
        float sunAngle = time * 360f;
        float moonAngle = (time + 0.5f) * 360f;

        if (sunLight != null)
        {
            sunLight.transform.rotation = Quaternion.Euler(new Vector3(sunAngle - 90f, 170f, 0f));

            float sunIntensity = CalculateSunIntensity(time);
            sunLight.intensity = sunIntensity;
            sunLight.color = sunColor.Evaluate(time);

            sunLight.shadows = sunIntensity > 0.2f ? LightShadows.Soft : LightShadows.None;
        }

        if (moonLight != null)
        {
            moonLight.transform.rotation = Quaternion.Euler(new Vector3(moonAngle - 90f, 170f, 0f));
            moonLight.intensity = CalculateMoonIntensity(time);

            // Делаем цвет луны более темным ночью
            float nightFactor = GetNightDarknessFactor(time);
            moonLight.color = Color.Lerp(
                new Color(0.7f, 0.8f, 1f),
                new Color(0.3f, 0.4f, 0.6f),
                nightFactor
            );
        }
    }

    float CalculateSunIntensity(float time)
    {
        float baseIntensity = sunIntensityCurve.Evaluate(time) * sunIntensityDay;

        // Применяем множитель темноты ночи
        float darknessFactor = GetNightDarknessFactor(time);
        baseIntensity *= (1f - darknessFactor * nightDarknessMultiplier);

        return Mathf.Lerp(baseIntensity, baseIntensity * 0.5f, bakedLightingWeight);
    }

    float CalculateMoonIntensity(float time)
    {
        if (time <= 0.25f || time >= 0.75f)
        {
            float darknessFactor = GetNightDarknessFactor(time);
            // Уменьшаем интенсивность луны в глубокую ночь
            return moonIntensityNight * (1f - bakedLightingWeight) * (1f - darknessFactor * 0.5f);
        }
        return moonIntensityDay;
    }

    void UpdateAmbientEnhancement(float time)
    {
        if (additionalAmbientLight != null)
        {
            float ambientBoost = ambientIntensityMultiplier.Evaluate(time);
            float sunIntensity = sunIntensityCurve.Evaluate(time);
            float darknessFactor = GetNightDarknessFactor(time);

            // Применяем темноту ночи к ambient свету
            ambientBoost *= (1f - darknessFactor * nightDarknessMultiplier);
            ambientBoost = Mathf.Max(ambientBoost, minAmbientAtNight);

            if (sunIntensity < 0.3f && ambientBoost > minAmbientAtNight)
            {
                additionalAmbientLight.enabled = true;
                additionalAmbientLight.intensity = ambientBoost * dynamicLightingWeight;

                // Делаем ambient свет более темным ночью
                Color ambientColor = Color.Lerp(
                    new Color(0.3f, 0.3f, 0.4f),
                    new Color(0.1f, 0.1f, 0.2f),
                    darknessFactor
                );
                additionalAmbientLight.color = Color.Lerp(ambientColor, Color.white, sunIntensity);
            }
            else
            {
                additionalAmbientLight.enabled = false;
            }
        }
    }

    // НОВЫЙ МЕТОД: Расчет фактора темноты ночи
    float GetNightDarknessFactor(float time)
    {
        if (!enableDeepNight) return 0f;

        // Рассчитываем насколько глубока ночь
        float nightTime = time;
        if (time > 0.5f) nightTime = 1f - time; // Нормализуем для второй половины ночи

        float deepNightStart = (0.5f - deepNightDuration) / 2f;
        float deepNightEnd = 0.5f - deepNightStart;

        if (nightTime >= deepNightStart && nightTime <= deepNightEnd)
        {
            // Глубокая ночь - максимальная темнота
            return 1f;
        }
        else if (nightTime < deepNightStart)
        {
            // Вечер - плавное увеличение темноты
            return nightTime / deepNightStart;
        }
        else
        {
            // Утро - плавное уменьшение темноты
            return (0.5f - nightTime) / deepNightStart;
        }
    }

    // НОВЫЙ МЕТОД: Применение темноты ночи
    void UpdateNightDarkness(float time)
    {
        float darknessFactor = GetNightDarknessFactor(time);

        // Применяем темноту к global ambient освещению
        if (useBakedAmbient)
        {
            float currentAmbient = RenderSettings.ambientIntensity;
            float targetAmbient = Mathf.Lerp(currentAmbient, minAmbientAtNight, darknessFactor * 0.5f);
            RenderSettings.ambientIntensity = Mathf.Max(targetAmbient, minAmbientAtNight);
        }
    }

    void UpdateLightProbes()
    {
        if (Time.frameCount % 120 == 0)
        {
            LightProbes.TetrahedralizeAsync();
        }
    }

    [ContextMenu("Setup for Dark Nights")]
    public void SetupForDarkNights()
    {
        // Кривая интенсивности солнца с резкими переходами
        sunIntensityCurve = new AnimationCurve(
            new Keyframe(0.0f, 0.0f),    // полночь
            new Keyframe(0.15f, 0.1f),   // поздняя ночь
            new Keyframe(0.2f, 0.4f),    // рассвет
            new Keyframe(0.25f, 0.8f),   // утро
            new Keyframe(0.4f, 0.95f),   // день
            new Keyframe(0.5f, 1.0f),    // полдень
            new Keyframe(0.6f, 0.95f),   // день
            new Keyframe(0.75f, 0.8f),   // вечер
            new Keyframe(0.8f, 0.4f),    // закат
            new Keyframe(0.85f, 0.1f),   // ранняя ночь
            new Keyframe(1.0f, 0.0f)     // полночь
        );

        // Градиент цвета солнца
        sunColor = new Gradient();
        sunColor.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(new Color(0.3f, 0.3f, 0.4f), 0.0f),   // глубокая ночь
            new GradientColorKey(new Color(0.4f, 0.4f, 0.5f), 0.15f),  // поздняя ночь
            new GradientColorKey(new Color(1.0f, 0.6f, 0.3f), 0.2f),   // рассвет
            new GradientColorKey(Color.white, 0.4f),                   // день
            new GradientColorKey(Color.white, 0.6f),                   // день
            new GradientColorKey(new Color(1.0f, 0.6f, 0.3f), 0.8f),   // закат
            new GradientColorKey(new Color(0.4f, 0.4f, 0.5f), 0.85f),  // ранняя ночь
            new GradientColorKey(new Color(0.3f, 0.3f, 0.4f), 1.0f)    // глубокая ночь
        };

        // Кривая ambient освещения с очень низкими значениями ночью
        ambientIntensityMultiplier = new AnimationCurve(
            new Keyframe(0.0f, 0.1f),    // глубокая ночь - очень темно
            new Keyframe(0.15f, 0.15f),  // поздняя ночь
            new Keyframe(0.2f, 0.3f),    // рассвет
            new Keyframe(0.4f, 0.4f),    // утро
            new Keyframe(0.5f, 0.5f),    // полдень
            new Keyframe(0.6f, 0.4f),    // день
            new Keyframe(0.8f, 0.3f),    // закат
            new Keyframe(0.85f, 0.15f),  // ранняя ночь
            new Keyframe(1.0f, 0.1f)     // глубокая ночь
        );

        // Кривая темноты ночи
        nightDarknessCurve = new AnimationCurve(
            new Keyframe(0.0f, 1.0f),    // полночь - максимальная темнота
            new Keyframe(0.15f, 0.8f),   // поздняя ночь
            new Keyframe(0.2f, 0.0f),    // рассвет - нет темноты
            new Keyframe(0.8f, 0.0f),    // закат - нет темноты
            new Keyframe(0.85f, 0.8f),   // ранняя ночь
            new Keyframe(1.0f, 1.0f)     // полночь
        );

        // Настройки для темной ночи
        nightDarknessMultiplier = 0.7f;
        minAmbientAtNight = 0.05f;
        enableDeepNight = true;
        deepNightDuration = 0.3f;
        moonIntensityNight = 0.08f; // Очень слабая луна
        sunIntensityDay = 2.0f; // Яркое солнце для контраста

        Debug.Log("Dark night setup complete! Night will be much darker now.");
    }

    [ContextMenu("Make Night Brighter")]
    public void MakeNightBrighter()
    {
        nightDarknessMultiplier = 0.3f;
        minAmbientAtNight = 0.2f;
        moonIntensityNight = 0.2f;
        UpdateLighting(currentTime / dayDuration);
        Debug.Log("Night brightness increased");
    }

    [ContextMenu("Make Night Darker")]
    public void MakeNightDarker()
    {
        nightDarknessMultiplier = 0.9f;
        minAmbientAtNight = 0.02f;
        moonIntensityNight = 0.05f;
        UpdateLighting(currentTime / dayDuration);
        Debug.Log("Night darkness increased");
    }

    void OnValidate()
    {
        dayDuration = Mathf.Max(dayDuration, 10f);
        updateInterval = Mathf.Max(updateInterval, 0.01f);
        dynamicLightingWeight = 1f - bakedLightingWeight;
        minAmbientAtNight = Mathf.Max(minAmbientAtNight, 0.01f);
    }

    public void SetTime(float normalizedTime)
    {
        currentTime = normalizedTime * dayDuration;
        UpdateLighting(normalizedTime);
    }

    public float GetCurrentTime() => currentTime / dayDuration;

    // Методы для регулировки темноты в runtime
    public void SetNightDarkness(float darkness)
    {
        nightDarknessMultiplier = Mathf.Clamp01(darkness);
        UpdateLighting(currentTime / dayDuration);
    }

    public void SetMinAmbient(float minAmbient)
    {
        minAmbientAtNight = Mathf.Clamp(minAmbient, 0.01f, 1f);
        UpdateLighting(currentTime / dayDuration);
    }
}