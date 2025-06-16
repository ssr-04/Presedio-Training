import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'highlight'
})
export class HighlightPipe implements PipeTransform {

  transform(value: string, searchTerm: string): string {
    if (!searchTerm || !value) {
      return value;
    }

    const re = new RegExp(searchTerm, 'gi'); // 'gi' -> case-insensitive global search
    return value.replace(re, match => `<span class="bg-yellow-200">${match}</span>`);
  }

}
