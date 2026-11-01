import { Routes } from '@angular/router';
import { ExamComponent } from './pages/exam/exam.component';

export const routes: Routes = [
  { path: '', component: ExamComponent, title: 'MiniExam | example.com' },
  { path: '**', redirectTo: '' },
];
