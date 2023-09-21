using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public GameObject healthIcon;                   // the icon to show for health
    public int iconsPerRow = 8;                     // how many icons to show per row before moving to the next
    private List<GameObject> healthIconSet;         // running list of the life bar icons - doesn't destroy just unenables when health is lost
    private float margin = 3;                       // margin in between icons

    
    // sets up the health bar with the full amount
    public void SetHealth(int maxAmt, int curAmt){
        // reset the health amount
        healthIconSet = new List<GameObject>();    

        // get the icon dimensions for spacing
        float sprWidth = healthIcon.GetComponent<RectTransform>().rect.width;
        float sprHeight = healthIcon.GetComponent<RectTransform>().rect.height;

        // make the full amount possible
        for(int i=0;i<maxAmt;i++){
            GameObject h = Instantiate(healthIcon);
            h.transform.parent = transform;
            h.transform.localScale = new Vector3(1,1,1);
            h.transform.localPosition = new Vector3(
                (sprWidth+margin)*(i%iconsPerRow),
                0-(sprHeight+margin)*Mathf.Floor(i/iconsPerRow),
                0);
            healthIconSet.Add(h);
        }

        // set to the current amount
        UpdateHealth(curAmt);
    }

    // updates the lives bar with the amount of health left
    public void UpdateHealth(int health){
        for(int i=0;i<healthIconSet.Count;i++){
            healthIconSet[i].SetActive(health >= (i+1));     // show the health for the amount available
        }
    }
}
