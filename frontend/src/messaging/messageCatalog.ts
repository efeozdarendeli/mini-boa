import type { tr } from './resources/tr';

/** tr.ts'nin yapısı; değerler sabit metin yerine herhangi bir string olabilir. */
export type MessageCatalog = {
  [Category in keyof typeof tr]: { [Key in keyof (typeof tr)[Category]]: string };
};

export type MessageCategory = keyof MessageCatalog;

export type MessageKey<Category extends MessageCategory> = keyof MessageCatalog[Category];
