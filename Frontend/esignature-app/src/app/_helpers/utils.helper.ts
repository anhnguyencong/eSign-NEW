
import { ValidatorFn, AbstractControl } from '@angular/forms';
import { PageListModel } from '../models/pagelist.model';

export function EmailFormatValidator(): ValidatorFn {
    return function validate(control: AbstractControl) {
        const regex = new RegExp(/^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/);
        if (!regex.test(control.value)) {
            return {
                email: true,
            };
        }
        return null;
    };
}

export function ReOrderSortNumber(model: PageListModel, event: any) {
    const dropItemSortNumber = model.items[event.dropIndex].sortNumber;
    if (event.dragIndex > event.dropIndex) {
        model.items.forEach((item, index) => {
            if (index >= event.dropIndex && index < event.dragIndex) {
                item.sortNumber = model.items[index + 1].sortNumber;
            }
            if (index === event.dragIndex) {
                item.sortNumber = dropItemSortNumber;
            }

            if (index > event.dragIndex) {
                return;
            }
        });
    } else {
        let previousNumber = 0;
        model.items.forEach((item, index) => {
            if (index === event.dragIndex) {
                previousNumber = item.sortNumber;
                item.sortNumber = dropItemSortNumber;
            }
            if (index > event.dragIndex && index <= event.dropIndex) {
                const temp = item.sortNumber;
                item.sortNumber = previousNumber;
                previousNumber = temp;
            }

            if (index > event.dropIndex) {
                return;
            }
        });
    }
}

export function FormatString(str: string, ...val: any[]) {
    for (let index = 0; index < val.length; index++) {
        str = str.replace(`{${index}}`, val[index]);
    }
    return str;
}

export function RandomEnum<T>(anEnum: T): T[keyof T] {
    const enumValues = Object.keys(anEnum)
      .map(n => Number.parseInt(n))
      .filter(n => !Number.isNaN(n)) as unknown as T[keyof T][]
    const randomIndex = Math.floor(Math.random() * enumValues.length)
    const randomEnumValue = enumValues[randomIndex]
    return randomEnumValue;
  }
