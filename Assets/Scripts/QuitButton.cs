using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuitButton : MyButton {
    
    public override void TaskOnClick() {
        LevelManager levelManager = LevelManager.Instance;
        levelManager.Quit();
    }
}
