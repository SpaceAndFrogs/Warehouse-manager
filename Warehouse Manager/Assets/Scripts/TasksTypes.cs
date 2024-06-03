using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "TaskData", menuName = "Tasks", order = 1)]
public class TasksTypes : ScriptableObject
{
    public enum TaskType {None, Go, Chop, Dry, Mine, Build}
    public List<Task> tasks = new List<Task>();

    [Serializable]
    public class Task
    {
        public float taskTime;
        public TaskType taskType;
        public Sprite buttonSprite;
        public string nameOfButton;
        public Task(float taskTime, TaskType taskType, Sprite buttonSprite, string nameOfButton)
        {
            this.taskTime = taskTime;
            this.taskType = taskType;
            this.buttonSprite = buttonSprite;
            this.nameOfButton = nameOfButton;
        }
    }
}
