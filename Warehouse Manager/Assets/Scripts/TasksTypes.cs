using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "TaskData", menuName = "Tasks", order = 1)]
public class TasksTypes : ScriptableObject
{
    public enum TaskType {None, Go, Chop, Dry, Mine, Build, Destroy}

    public enum TaskClass {Build, Pick, Pack}
    public List<Task> tasks = new List<Task>();

    [Serializable]
    public class Task
    {
        public float taskTime;
        public float cost;
        public TaskType taskType;
        public TaskClass taskClass;
        public Sprite buttonSprite;
        public string nameOfButton;

        public TileTypes.TileType tileTypeAfterTask;

        public Task(TaskClass taskClass)
        {
            this.taskClass = taskClass;
        }

    }
}
