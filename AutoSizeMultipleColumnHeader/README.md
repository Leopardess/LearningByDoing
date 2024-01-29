C# AutoSize Multiple Column Header
-----------------------------------------------------------------------
本項是被要求使用 Rectangle 在 dataGridView 繪製多重表頭，其中涵蓋：
1. 主表頭和子表頭
2. 合併儲存格式的主表頭
3. 某些子表頭沒有文字，僅主表有文字
4. 每欄表頭文字長度不同、欄寬也不同
5. 子表頭無文字，或文字長度較主表頭短時，參照主表頭文字長度定義欄寬；其他時候以子表頭文字長度為主
6. 每次查詢時欄位數量不等，依照撈取資料結果自動定義

-----------------------------------------------------------------------
This project makes me need to draw a Multiple Column Header using Rectangle. Includes:
1. Main and Sub Column Header
2. Merged Multiple Column Main Header
3. Sub Column Header may be "", only Main Column Header exists charaters
4. Every Column Header's Text is not the same length like others, which leads to different column widths
5. If Sub Column Header is "", or the length of Sub Column Header's Text is shorter than Main Column Header, use Main Column Header's Text length;
   Otherwise, use Sub Main Column Header's Text length
6. Each query may leads to different columns amount. That's why it should be auto generate.
