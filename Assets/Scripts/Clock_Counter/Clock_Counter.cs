using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Clock_Counter : MonoBehaviour
{
    [SerializeField] private GameObject Witness_Button;
    [SerializeField] private GameObject Ask_Murder_Button;
    [SerializeField] private TextMeshProUGUI Placeholder;
    [SerializeField] private GameObject Input_Field;

    public int Start_Clock = 0000;
    public int End_Clock = 0900;
    public int Current_Clock;

    [SerializeField] private TextMeshProUGUI XY_YY; // saat onlar
    [SerializeField] private TextMeshProUGUI YX_YY; // saat birler
    [SerializeField] private TextMeshProUGUI YY_XY; // dakika onlar
    [SerializeField] private TextMeshProUGUI YY_YX; // dakika birler

    private Coroutine clockAnimationCoroutine;

    void Awake()
    {
        Update_Clock_TMPS();
    }

    public void Set_Current_CLock_Var()
    {
        int minutes = Current_Clock % 100;
        int previousClock = Current_Clock;

        if (minutes == 0)
        {
            Current_Clock += 30; // örn. 08:00 -> 08:30
        }
        else if (minutes == 30)
        {
            Current_Clock += 70; // örn. 08:30 -> 09:00 (sonrasında normalize edeceğiz)
        }
        else
        {
            if (minutes < 30) Current_Clock = (Current_Clock / 100) * 100 + 30;
            else Current_Clock = (Current_Clock / 100) * 100 + 100;
        }

        NormalizeClock();
        ClampToEndClock();

        // Önceki saat ile yeni saat arasında animasyon başlat
        if (clockAnimationCoroutine != null)
            StopCoroutine(clockAnimationCoroutine);

        clockAnimationCoroutine = StartCoroutine(AnimateClockChange(previousClock, Current_Clock));
    }
    
    public void Set_Current_CLock_Var_For_Witness()
    {
        int minutes = Current_Clock % 100;
        int previousClock = Current_Clock;

        Current_Clock += 100;

        NormalizeClock();
        ClampToEndClock();

        // Önceki saat ile yeni saat arasında animasyon başlat
        if (clockAnimationCoroutine != null)
            StopCoroutine(clockAnimationCoroutine);

        clockAnimationCoroutine = StartCoroutine(AnimateClockChange(previousClock, Current_Clock));
    }

    private void NormalizeClock()
    {
        int hours = Current_Clock / 100;
        int minutes = Current_Clock % 100;

        if (minutes >= 60)
        {
            hours += minutes / 60;
            minutes = minutes % 60;
        }

        Current_Clock = hours * 100 + minutes;
    }

    private void ClampToEndClock()
    {
        if (Current_Clock >= End_Clock)
        {
            Current_Clock = End_Clock;
            Placeholder.text = "    !! Avukat geldi lütfen son tahmininzde bulunun !! \n --------------------------------------------------------";
            Input_Field.SetActive(false);
            Witness_Button.SetActive(false);
            Ask_Murder_Button.SetActive(false);
        }
    }

    public void Update_Clock_TMPS()
    {
        int hours = Current_Clock / 100;
        int minutes = Current_Clock % 100;

        int hourTens = (hours / 10) % 10;
        int hourOnes = hours % 10;
        int minuteTens = (minutes / 10) % 10;
        int minuteOnes = minutes % 10;

        if (XY_YY) XY_YY.text = hourTens.ToString();
        if (YX_YY) YX_YY.text = hourOnes.ToString();
        if (YY_XY) YY_XY.text = minuteTens.ToString();
        if (YY_YX) YY_YX.text = minuteOnes.ToString();
    }

    private IEnumerator AnimateClockChange(int oldClock, int newClock)
    {
        int oldHours = oldClock / 100;
        int oldMinutes = oldClock % 100;
        int newHours = newClock / 100;
        int newMinutes = newClock % 100;

        // Dakika aralığını belirle
        int startMinute = oldMinutes;
        int endMinute = newMinutes;

        // Eğer saat değişiyorsa 30 → 59 → 00 şeklinde animasyon
        if (oldHours != newHours)
        {
            for (int m = startMinute + 1; m < 60; m++)
            {
                DisplayTime(oldHours, m);
                yield return new WaitForSeconds(0.05f);
            }
            oldHours = newHours;
            startMinute = 0;
        }

        // Şimdi yeni saate ait dakikaları göster
        for (int m = startMinute; m <= endMinute; m++)
        {
            DisplayTime(oldHours, m);
            yield return new WaitForSeconds(0.05f);
        }

        Current_Clock = newClock;
        Update_Clock_TMPS();
    }

    private void DisplayTime(int hours, int minutes)
    {
        int hourTens = (hours / 10) % 10;
        int hourOnes = hours % 10;
        int minuteTens = (minutes / 10) % 10;
        int minuteOnes = minutes % 10;

        if (XY_YY) XY_YY.text = hourTens.ToString();
        if (YX_YY) YX_YY.text = hourOnes.ToString();
        if (YY_XY) YY_XY.text = minuteTens.ToString();
        if (YY_YX) YY_YX.text = minuteOnes.ToString();
    }
}
