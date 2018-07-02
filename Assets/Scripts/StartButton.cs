using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartButton : MyButton {

    public override void TaskOnClick() {
        LevelManager levelManager = LevelManager.Instance;
        levelManager.LoadScene("main");
    }
}
