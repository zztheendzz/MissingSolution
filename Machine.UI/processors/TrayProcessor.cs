using Machine.UI.model;
using System;
using System.Collections.Generic;

public class TrayProcessor
{
    private readonly TrayModel tray;

    private int currentBatch = 0;

    private int rows;
    private int cols;

    // số phần tử mỗi cụm
    public int batchSize = 5;

    private int totalBatch;

    public bool IsFull => currentBatch >= totalBatch;

    public TrayProcessor(TrayModel tray)
    {
        this.tray = tray;

        rows = tray.Rows;
        cols = tray.Cols;

        int batchPerCol = rows / batchSize;
        int remainder = rows % batchSize;

        int totalBatchPerCol =
            batchPerCol + (remainder > 0 ? 1 : 0);

        totalBatch = totalBatchPerCol * cols;
    }

    public void Reset()
    {
        currentBatch = 0;
    }

    //====================================================
    // Robot chạy:
    //
    // cột cuối -> cột đầu
    //
    // zigzag:
    // trên -> dưới
    // dưới -> trên
    //
    // xử lý đúng cả batch dư cuối cột
    //====================================================

    public List<Cell> 
        ProcessBatch(List<string> results)
    {
        List<Cell> output = new List<Cell>();

        int batchPerCol = rows / batchSize;
        int remainder = rows % batchSize;

        int totalBatchPerCol =
            batchPerCol + (remainder > 0 ? 1 : 0);

        // robot chạy từ phải -> trái
        int colIndexFromRight =
            currentBatch / totalBatchPerCol;

        //int currentCol =
        //    cols - 1 - colIndexFromRight;

        int currentCol = colIndexFromRight;

        int batchInCol =
            currentBatch % totalBatchPerCol;

        if (currentCol < 0)
            return output;

        //================================================
        // zigzag
        //================================================

        bool isForward =
            colIndexFromRight % 2 == 0;

        //================================================
        // tính số lượng phần tử của batch
        //================================================

        int currentBatchSize;

        if (isForward)
        {
            // trên -> dưới

            if (batchInCol < batchPerCol)
                currentBatchSize = batchSize;
            else
                currentBatchSize = remainder;
        }
        else
        {
            // dưới -> trên

            // batch đầu tiên sẽ lấy phần dư
            if (remainder > 0 && batchInCol == 0)
            {
                currentBatchSize = remainder;
            }
            else
            {
                currentBatchSize = batchSize;
            }
        }

        if (currentBatchSize == 0)
            currentBatchSize = batchSize;

        //================================================
        // tính vị trí bắt đầu
        //================================================

        int startRow;

        if (isForward)
        {
            // trên -> dưới

            startRow = batchInCol * batchSize;
        }
        else
        {
            // dưới -> trên

            if (remainder > 0)
            {
                if (batchInCol == 0)
                {
                    // cụm dư cuối
                    startRow = rows - 1;
                }
                else
                {
                    startRow =
                        rows - 1
                        - remainder
                        - ((batchInCol - 1) * batchSize);
                }
            }
            else
            {
                startRow =
                    rows - 1
                    - (batchInCol * batchSize);
            }
        }

        //================================================
        // build output
        //================================================

        for (int i = 0; i < currentBatchSize; i++)
        {
            if (i >= results.Count)
                break;

            int rowIndex;

            if (isForward)
            {
                rowIndex = startRow + i;
            }
            else
            {
                rowIndex = startRow - i;
            }

            string result;

            // giữ thứ tự đúng khi đảo chiều
            if (isForward)
            {
                result = results[i];
            }
            else
            {
                result =
                    results[currentBatchSize - 1 - i];
            }

            output.Add(new Cell
            {
                Row = rowIndex,
                Col = currentCol,
                Result = result
            });
        }
        
        Console.WriteLine(
            $"Batch={currentBatch}, Col={currentCol}, StartRow={startRow}, Forward={isForward}");

        currentBatch++;

        return output;
    }
}