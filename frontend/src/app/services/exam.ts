import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Question } from '../models/question';
import { SubmitExam } from '../models/submit-exam';
import { ExamResult } from '../models/exam-result';

@Injectable({ providedIn: 'root' })
export class Exam {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/exam';

  getQuestions() {
    return this.http.get<Question[]>(`${this.baseUrl}/questions`);
  }

  submit(request: SubmitExam) {
    return this.http.post<ExamResult>(`${this.baseUrl}/submit`, request);
  }

  getResult(id: number) {
    return this.http.get<ExamResult>(`${this.baseUrl}/results/${id}`);
  }
}
