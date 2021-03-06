﻿//   Copyright (c) Thortech Solutions, LLC. All rights reserved.
//   The use and distribution terms for this software are covered by the
//   Eclipse Public License 1.0 (http://opensource.org/licenses/eclipse-1.0.php)
//   which can be found in the file epl-v10.html at the root of this distribution.
//   By using this software in any fashion, you are agreeing to be bound by
//   the terms of this license.
//   You must not remove this notice, or any other, from this software.
//
//   Authors: Dimitrios Kapsalis, Mark Perrotta
//

using System.IO;
using System.Collections;

namespace EDNTypes
{
    public interface IWriteHandler
    {
        void handleObject(object obj, Stream stream);
        void handleObject(object obj, Stream stream,object parent);
        void handleEnumerable(IEnumerable enumerable, Stream stream,object parent);
        void handleEnumerable(IEnumerable enumerable, Stream stream);
    }
}
