import { Component, DestroyRef, inject, OnInit, signal, ViewChild } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { Exam } from '../../services/exam';
import { Question } from '../../models/question';
import { ExamResult } from '../../models/exam-result';

@Component({
  imports: [FormsModule],
  selector: 'app-exam',
  styleUrl: './exam.component.css',
  templateUrl: './exam.component.html',
})
export class ExamComponent implements OnInit {
  private readonly exam = inject(Exam);
  private readonly destroyRef = inject(DestroyRef);
  @ViewChild(NgForm) private form?: NgForm;

  readonly questions = signal<Question[]>([]);
  readonly fullName = signal('');
  readonly selectedAnswers = signal<Record<number, number>>({});
  readonly result = signal<ExamResult | null>(null);
  readonly loading = signal(true);
  readonly submitting = signal(false);
  readonly error = signal('');
  readonly attempted = signal(false);

  ngOnInit(): void {
    this.loadQuestions();
  }

  loadQuestions(): void {
    this.loading.set(true);
    this.error.set('');
    this.exam.getQuestions().pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false)),
    ).subscribe({
      next: questions => this.questions.set(questions),
      error: () => this.error.set('โหลดข้อสอบไม่สำเร็จ กรุณาลองอีกครั้ง'),
    });
  }

  answerFor(questionId: number): number | undefined {
    const result = this.result();
    return result
      ? result.answers.find(answer => answer.questionId === questionId)?.selectedOptionId
      : this.selectedAnswers()[questionId];
  }

  selectAnswer(questionId: number, optionId: number): void {
    if (this.submitting() || this.result()) return;
    this.selectedAnswers.update(answers => ({ ...answers, [questionId]: optionId }));
    this.error.set('');
  }

  submit(): void {
    if (this.loading() || this.submitting() || this.result()) return;
    this.attempted.set(true);
    this.error.set('');
    const fullName = this.fullName().trim();
    if (!fullName || this.fullName().length > 200 || !this.questions().length
      || this.questions().some(question => !this.answerFor(question.id))) return;

    this.submitting.set(true);
    this.exam.submit({
      fullName,
      answers: this.questions().map(question => ({
        questionId: question.id,
        selectedOptionId: this.selectedAnswers()[question.id],
      })),
    }).pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.submitting.set(false)),
    ).subscribe({
      next: result => {
        this.fullName.set(result.fullName);
        this.result.set(result);
        this.attempted.set(false);
      },
      error: (error: HttpErrorResponse) => this.error.set(
        error.status === 400 || error.status === 409
          ? error.error?.detail ?? 'ข้อมูลไม่ถูกต้อง กรุณาตรวจสอบชื่อและคำตอบ'
          : 'ส่งข้อสอบไม่สำเร็จ กรุณาลองอีกครั้ง',
      ),
    });
  }

  retake(): void {
    if (this.submitting()) return;
    this.result.set(null);
    this.fullName.set('');
    this.selectedAnswers.set({});
    this.attempted.set(false);
    this.error.set('');
    this.form?.resetForm({ fullName: '' });
  }
}
