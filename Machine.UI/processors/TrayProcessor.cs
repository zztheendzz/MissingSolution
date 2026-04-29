using Machine.UI.model;
using System;
using System.Collections.Generic;

public class TrayProcessor
{
    private readonly TrayModel tray;
    private int currentBatch = 0;

    private int rows;
    private int cols;

    private int batchSize = 5;

    public bool IsFull => currentBatch >= totalBatch;

    private int totalBatch;

    public TrayProcessor(TrayModel tray)
    {
        this.tray = tray;
        rows = tray.Rows;
        cols = tray.Cols;

        int batchPerRow = cols / batchSize;
        int remainder = cols % batchSize;

        int totalBatchPerRow = batchPerRow + (remainder > 0 ? 1 : 0);

        totalBatch = totalBatchPerRow * rows;
    }

    public void Reset()
    {
        currentBatch = 0;
    }

    public List<Cell> ProcessBatch(List<string> results)
    {
        List<Cell> output = new List<Cell>();

        int batchPerRow = cols / batchSize;
        int remainder = cols % batchSize;

        int totalBatchPerRow = batchPerRow + (remainder > 0 ? 1 : 0);

        int currentRow = currentBatch / totalBatchPerRow;
        int batchInRow = currentBatch % totalBatchPerRow;

        if (currentRow >= rows) return output;

        int currentBatchSize =
            (batchInRow < batchPerRow) ? batchSize : remainder;

        if (currentBatchSize == 0) currentBatchSize = batchSize;

        int startCol;

        if (currentRow % 2 == 0)
        {
            // → trái sang phải
            startCol = batchInRow * batchSize;
        }
        else
        {
            // ← phải sang trái
            startCol = cols - (batchInRow * batchSize) - currentBatchSize;
        }

        for (int i = 0; i < currentBatchSize; i++)
        {
            if (i >= results.Count) break;

            int colIndex = startCol + i;

            output.Add(new Cell
            {
                Row = currentRow,
                Col = colIndex,
                Result = results[i]
            });
        }

        // 🔥 debug cực quan trọng
        Console.WriteLine($"Batch={currentBatch}, Row={currentRow}, StartCol={startCol}");

        currentBatch++;

        return output;
    }
}