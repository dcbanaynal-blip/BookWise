import * as react_jsx_runtime from 'react/jsx-runtime';
import { ReactNode } from 'react';

type StatBadgeProps = {
    label: string;
    value: ReactNode;
    tone?: 'primary' | 'success' | 'warning';
};
declare function StatBadge({ label, value, tone }: StatBadgeProps): react_jsx_runtime.JSX.Element;

export { StatBadge, type StatBadgeProps };
