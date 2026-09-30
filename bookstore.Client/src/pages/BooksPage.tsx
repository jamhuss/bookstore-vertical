import { useState } from 'react';
import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { booksApi, getApiErrorMessage } from '../api';
import type { Book } from '../types';
import { BookModal } from '../components/BookModal';

const pageSize = 5;

export function BooksPage() {
    const queryClient = useQueryClient();
    const [page, setPage] = useState(1);

    const [modalOpen, setModalOpen] = useState(false);
    const [editingBook, setEditingBook] = useState<Book | null>(null);

    const { data, isPending, error } = useQuery({
        queryKey: ['books', page],
        queryFn: () => booksApi.list(page, pageSize),
        placeholderData: keepPreviousData,
    });

    const deleteMutation = useMutation({
        mutationFn: (book: Book) => booksApi.remove(book.id),
        onSuccess: () => {
            if (data && data.items.length === 1 && page > 1) {
                setPage(page - 1);
            } else {
                queryClient.invalidateQueries({ queryKey: ['books'] });
            }
        },
    });

    function openCreate() {
        setEditingBook(null);
        setModalOpen(true);
    }

    function openEdit(book: Book) {
        setEditingBook(book);
        setModalOpen(true);
    }

    function handleSaved() {
        setModalOpen(false);
        queryClient.invalidateQueries({ queryKey: ['books'] });
    }

    function remove(book: Book) {
        if (!confirm(`Delete "${book.title}"?`)) {
            return;
        }
        deleteMutation.mutate(book);
    }

    const errorMessage = error
        ? getApiErrorMessage(error)
        : deleteMutation.error
          ? getApiErrorMessage(deleteMutation.error)
          : null;

    return (
        <div>
            <div className="page-header">
                <h1>Books</h1>
                <button type="button" className="primary" onClick={openCreate}>
                    + New book
                </button>
            </div>

            {errorMessage && <p className="error">{errorMessage}</p>}
            {isPending && <p>Loading…</p>}

            {data && (
                <>
                    <table className="data-table">
                        <thead>
                            <tr>
                                <th>Title</th>
                                <th>Author</th>
                                <th>Genre</th>
                                <th className="num">Price</th>
                                <th className="actions">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            {data.items.map((b) => (
                                <tr key={b.id}>
                                    <td>{b.title}</td>
                                    <td>{b.author}</td>
                                    <td>
                                        <span className="badge">{b.genre}</span>
                                    </td>
                                    <td className="num">{b.price.toFixed(2)} kr</td>
                                    <td className="actions">
                                        <button type="button" onClick={() => openEdit(b)}>
                                            Edit
                                        </button>
                                        <button
                                            type="button"
                                            className="danger"
                                            onClick={() => remove(b)}
                                        >
                                            Delete
                                        </button>
                                    </td>
                                </tr>
                            ))}
                            {data.items.length === 0 && (
                                <tr>
                                    <td colSpan={5} className="empty">
                                        No books yet.
                                    </td>
                                </tr>
                            )}
                        </tbody>
                    </table>

                    <div className="pager">
                        <button
                            type="button"
                            disabled={!data.hasPrevious}
                            onClick={() => setPage((p) => p - 1)}
                        >
                            ‹ Prev
                        </button>
                        <span>
                            Page {data.page} of {data.totalPages || 1} · {data.totalCount} total
                        </span>
                        <button
                            type="button"
                            disabled={!data.hasNext}
                            onClick={() => setPage((p) => p + 1)}
                        >
                            Next ›
                        </button>
                    </div>
                </>
            )}

            {modalOpen && (
                <BookModal
                    book={editingBook}
                    onClose={() => setModalOpen(false)}
                    onSaved={handleSaved}
                />
            )}
        </div>
    );
}
