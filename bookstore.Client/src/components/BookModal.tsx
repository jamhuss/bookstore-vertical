import { useEffect, useState } from 'react';
import { booksApi, getApiErrorMessage } from '../api';
import type { Book, BookInput } from '../types';
import { Modal } from './Modal';

const emptyForm: BookInput = {
    title: '',
    author: '',
    genre: '',
    description: '',
    price: 0,
};

interface BookModalProps {
    open: boolean;
    book: Book | null;
    onClose: () => void;
    onSaved: () => void;
}


export function BookModal({ open, book, onClose, onSaved }: BookModalProps) {
    const [form, setForm] = useState<BookInput>(emptyForm);
    const [error, setError] = useState<string | null>(null);
    const [saving, setSaving] = useState(false);

    useEffect(() => {
        if (!open) {
            return;
        }
        setForm(
            book
                ? {
                      title: book.title,
                      author: book.author,
                      genre: book.genre,
                      description: book.description,
                      price: book.price,
                  }
                : emptyForm,
        );
        setError(null);
    }, [open, book]);

    async function save(e: React.FormEvent) {
        e.preventDefault();
        setSaving(true);
        setError(null);
        try {
            if (book) {
                await booksApi.update(book.id, form);
            } else {
                await booksApi.create(form);
            }
            onSaved();
        } catch (err) {
            setError(getApiErrorMessage(err));
        } finally {
            setSaving(false);
        }
    }

    return (
        <Modal title={book ? 'Edit book' : 'New book'} open={open} onClose={onClose}>
            <form onSubmit={save} className="form">
                <label>
                    Title
                    <input
                        value={form.title}
                        onChange={(e) => setForm({ ...form, title: e.target.value })}
                    />
                </label>
                <label>
                    Author
                    <input
                        value={form.author}
                        onChange={(e) => setForm({ ...form, author: e.target.value })}
                    />
                </label>
                <label>
                    Genre
                    <input
                        value={form.genre}
                        onChange={(e) => setForm({ ...form, genre: e.target.value })}
                    />
                </label>
                <label>
                    Description
                    <textarea
                        value={form.description}
                        rows={3}
                        onChange={(e) => setForm({ ...form, description: e.target.value })}
                    />
                </label>
                <label>
                    Price
                    <input
                        type="number"
                        step="0.01"
                        min="0"
                        value={form.price}
                        onChange={(e) => setForm({ ...form, price: Number(e.target.value) })}
                    />
                </label>

                {error && <p className="error">{error}</p>}

                <div className="form-actions">
                    <button type="button" onClick={onClose} disabled={saving}>
                        Cancel
                    </button>
                    <button type="submit" className="primary" disabled={saving}>
                        {saving ? 'Saving…' : 'Save'}
                    </button>
                </div>
            </form>
        </Modal>
    );
}
