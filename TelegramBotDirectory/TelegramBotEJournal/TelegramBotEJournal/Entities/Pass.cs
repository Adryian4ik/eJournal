// /*
//     Copyright © 2023 Anderwafe. All rights reserved.
// 
//     Copyright 2023 Anderwafe
// 
//     Licensed under the Apache License, Version 2.0 (the "License");
//     you may not use this file except in compliance with the License.
//     You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
//     Unless required by applicable law or agreed to in writing,
//     software distributed under the License is distributed on an "AS IS" BASIS,
//     WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//     See the License for the specific language governing permissions and limitations under the License.
// */

namespace TelegramBotEJournal.Entities;

public class Pass
{
    private long id;
    private long studentID;
    private byte day;
    private byte month;
    private long year;
    private byte lessonNumber;
    private long lessonID;
    private byte passType;
    private long documentID;

    public Pass() : this(-1, -1, 0, 0, 0, 0, -1, 255, -1) { }
    
    public Pass(long id, long studentId, byte day, byte month, long year, byte lessonNumber, long lessonId, byte passType, long documentId)
    {
        this.id = id;
        studentID = studentId;
        this.day = day;
        this.month = month;
        this.year = year;
        this.lessonNumber = lessonNumber;
        lessonID = lessonId;
        this.passType = passType;
        documentID = documentId;
    }

    public override string ToString()
    {
        return $"[{day}:{month}:{year}]\nЗанятие: {lessonID}\nТип пропуска: {(passType == 0 ? "Неуважительный" : (passType == 1 ? "Уважительный" : (passType == 2 ? "По справке" : "Not defined")))}";
    }
}