﻿using Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestStatus : MonoBehaviour {

	public Image[] statusImage;

	private NpcQuestStatus questStatus;

	// Use this for initialization
	void Start () {
		
	}
	
	public void SetQuestStatus(NpcQuestStatus status)
    {
		this.questStatus = status;

		for (int i = 0; i < 4; i++)
        {
			if (this.statusImage[i] != null)
            {
				this.statusImage[i].gameObject.SetActive(i == (int)status);
            }
        }
    }
}
