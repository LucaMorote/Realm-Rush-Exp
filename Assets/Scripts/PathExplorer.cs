﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PathExplorer : MonoBehaviour {

    [SerializeField] [Range(0f, 2f)] float iterationDelay  = 0.1f;
    [SerializeField] int iteration = 0;
    [SerializeField] Block startBlock, endBlock;

    enum State { NotRun, Running, Finished }; // todo detect success vs failure
    [SerializeField] State state = State.NotRun;

    Dictionary<Vector2Int, Block> blocks = new Dictionary<Vector2Int, Block>();
    Queue<Block> queue = new Queue<Block>();

    Vector2Int[] directions = {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

	// Use this for initialization
	void Start ()
    {
        Debug.Assert(startBlock, "No start block found");
        Debug.Assert(endBlock, "No end block found");
        LoadBlocks();
        StartCoroutine(ExploreAllNodes());
    }

    private void LoadBlocks()
    {
        var blockObjects = FindObjectsOfType(typeof(Block));
        foreach (Block blockObject in blockObjects)
        {
            Vector2Int gridPos = blockObject.GetGridPos();
            bool positionOccupied = blocks.ContainsKey(gridPos);
            Debug.Assert(!positionOccupied, "Overlapping block " + blockObject);
            blocks.Add(gridPos, blockObject);
        }
    }
   
    private IEnumerator ExploreAllNodes()
    {
        queue.Enqueue(startBlock);
        state = State.Running;

        while (queue.Count > 0)
        {
            var blockToSearchFrom = queue.Dequeue();

            if (blockToSearchFrom == endBlock)
            {
                break;
            }

            SearchFromPosition(blockToSearchFrom);
            yield return new WaitForSeconds(iterationDelay);
        }
        state = State.Finished;
        yield return new WaitForEndOfFrame();
    }

    private void SearchFromPosition(Block searchBlock)
    {
        Vector2Int searchCenter = searchBlock.GetGridPos();
        foreach (Vector2Int direction in directions)
        {
            Vector2Int searchPosition = searchCenter + direction;
            if (IsNeighbourUnexplored(searchPosition))
            {
                Block newBlockToSearch = blocks[searchPosition];
                queue.Enqueue(newBlockToSearch);
                searchBlock.SetExplored();
                newBlockToSearch.SetExploredFrom(searchBlock);
            }
        }
    }

    private bool IsNeighbourUnexplored(Vector2Int searchPosition)
    {
        Block targetBlock;
        try
        {
            targetBlock = blocks[searchPosition];
        }
        catch
        {
            return false;
        }
        finally
        {
            iteration++;
        }

        bool isBlockAtTarget = blocks.ContainsKey(searchPosition);
        bool targetIsUnexplored = !targetBlock.IsExplored();
        bool targetIsNotBlocked = !targetBlock.IsBlocked();

        if (isBlockAtTarget && targetIsUnexplored && targetIsNotBlocked)
        {
            return true;
        }
        return false;
    }
}