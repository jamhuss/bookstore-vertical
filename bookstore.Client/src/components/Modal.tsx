import type { ReactNode } from 'react';

interface ModalProps {
    title: string;
    open: boolean;
    onClose: () => void;
    children: ReactNode;
}

export function Modal({ title, open, onClose, children }: ModalProps) {
    if (!open) {
        return null;
    }

    return (
        <div className="modal-backdrop" onClick={onClose}>
            <div
                className="modal"
                role="dialog"
                aria-modal="true"
                onClick={(e) => e.stopPropagation()}
            >
                <header className="modal-header">
                    <h2>{title}</h2>
                    <button
                        type="button"
                        className="icon-button"
                        aria-label="Close"
                        onClick={onClose}
                    >
                        &times;
                    </button>
                </header>
                <div className="modal-body">{children}</div>
            </div>
        </div>
    );
}
